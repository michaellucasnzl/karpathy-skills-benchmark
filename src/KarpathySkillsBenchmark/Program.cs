using System.CommandLine;
using System.Text.Json;
using FluentValidation;
using KarpathySkillsBenchmark.Configuration;
using KarpathySkillsBenchmark.Fixtures;
using KarpathySkillsBenchmark.Metrics;
using KarpathySkillsBenchmark.Reporting;
using KarpathySkillsBenchmark.Runners;
using KarpathySkillsBenchmark.Storage;
using KarpathySkillsBenchmark.Storage.Entities;
using KarpathySkillsBenchmark.Tasks;
using KarpathySkillsBenchmark.Verification;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Spectre.Console;

namespace KarpathySkillsBenchmark;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var repoRoot = Directory.GetCurrentDirectory();
        var builder = Host.CreateApplicationBuilder();
        builder.Configuration
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddEnvironmentVariables();

        ConfigureLogging(repoRoot);
        RegisterServices(builder.Services, builder.Configuration, repoRoot);

        using var host = builder.Build();

        var root = new RootCommand("Benchmark Karpathy-style coding tasks with OpenCode and Venice AI.");
        root.AddCommand(BuildRunCommand(host.Services, repoRoot));
        root.AddCommand(BuildReportCommand(host.Services, repoRoot));
        root.AddCommand(BuildListTasksCommand(host.Services, repoRoot));
        root.AddCommand(BuildValidateTasksCommand(host.Services, repoRoot));
        root.AddCommand(BuildInitSkillsCommand(repoRoot));
        root.AddCommand(BuildHistoryCommand(host.Services));

        return await root.InvokeAsync(args);
    }

    private static void RegisterServices(IServiceCollection services, IConfiguration configuration, string repoRoot)
    {
        services.AddSingleton(configuration);
        services.AddSingleton(configuration.GetSection("Benchmark").Get<BenchmarkOptions>() ?? new BenchmarkOptions());
        services.AddSingleton<IValidator<TaskDefinition>, TaskDefinitionValidator>();
        services.AddSingleton<TaskCatalog>();
        services.AddSingleton<FixtureManager>();
        services.AddSingleton<IAgentRunner, OpenCodeRunner>();
        services.AddSingleton<IReportRenderer, MarkdownReportRenderer>();
        services.AddSingleton<IReportRenderer, HtmlReportRenderer>();
        services.AddSingleton<IReportRenderer, CsvReportRenderer>();
        services.AddSingleton<IMetricCollector, DiffMetricCollector>();
        services.AddSingleton<IMetricCollector, TimingMetricCollector>();
        services.AddSingleton<IMetricCollector, ComplexityMetricCollector>();
        services.AddSingleton<IMetricCollector, BehaviorMetricCollector>();
        services.AddSingleton<IReadOnlyDictionary<string, PricingRate>>(sp => configuration.GetSection("Pricing").GetChildren().ToDictionary(
            child => child.Key,
            child => new PricingRate(child.GetValue<decimal>("inputPer1k"), child.GetValue<decimal>("outputPer1k"))));
        services.AddSingleton<IMetricCollector>(sp => new TokenMetricCollector(sp.GetRequiredService<IReadOnlyDictionary<string, PricingRate>>()));
        services.AddSingleton<TestRunnerVerifier>();
        services.AddHttpClient();

        var benchmarkOptions = configuration.GetSection("Benchmark").Get<BenchmarkOptions>() ?? new BenchmarkOptions();
        var dbPath = Path.Combine(repoRoot, benchmarkOptions.ResultsDirectory, "benchmark.db");
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
        services.AddDbContext<BenchmarkDbContext>(options => options.UseSqlite($"Data Source={dbPath}"));
        services.AddScoped<RunRepository>();
    }

    private static void ConfigureLogging(string repoRoot)
    {
        Directory.CreateDirectory(Path.Combine(repoRoot, "results"));
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File(Path.Combine(repoRoot, "results", "benchmark.log"), rollingInterval: RollingInterval.Day)
            .CreateLogger();
    }

    private static Command BuildRunCommand(IServiceProvider services, string repoRoot)
    {
        var command = new Command("run", "Run the benchmark across one or more tasks.");
        var agentOption = new Option<string>("--agent", () => "opencode");
        var modelOption = new Option<string?>("--model");
        var tasksOption = new Option<string?>("--tasks");
        var repeatsOption = new Option<int>("--repeats", () => 1);
        var withSkillsOption = new Option<string?>("--with-skills");
        var outputOption = new Option<string?>("--output");
        var judgeModelOption = new Option<string?>("--judge-model");
        var timeoutOption = new Option<int?>("--timeout");
        var skipJudgeOption = new Option<bool>("--skip-judge");
        var onlyWithoutOption = new Option<bool>("--only-without");
        var onlyWithOption = new Option<bool>("--only-with");
        command.AddOption(agentOption);
        command.AddOption(modelOption);
        command.AddOption(tasksOption);
        command.AddOption(repeatsOption);
        command.AddOption(withSkillsOption);
        command.AddOption(outputOption);
        command.AddOption(judgeModelOption);
        command.AddOption(timeoutOption);
        command.AddOption(skipJudgeOption);
        command.AddOption(onlyWithoutOption);
        command.AddOption(onlyWithOption);

        command.SetHandler(async context =>
        {
            var agent = context.ParseResult.GetValueForOption(agentOption) ?? "opencode";
            var model = context.ParseResult.GetValueForOption(modelOption);
            var tasks = context.ParseResult.GetValueForOption(tasksOption);
            var repeats = context.ParseResult.GetValueForOption(repeatsOption);
            var withSkills = context.ParseResult.GetValueForOption(withSkillsOption);
            var output = context.ParseResult.GetValueForOption(outputOption);
            var judgeModel = context.ParseResult.GetValueForOption(judgeModelOption);
            var timeout = context.ParseResult.GetValueForOption(timeoutOption);
            var skipJudge = context.ParseResult.GetValueForOption(skipJudgeOption);
            var onlyWithout = context.ParseResult.GetValueForOption(onlyWithoutOption);
            var onlyWith = context.ParseResult.GetValueForOption(onlyWithOption);

            if (onlyWith && onlyWithout)
            {
                throw new InvalidOperationException("--only-with and --only-without cannot be used together.");
            }

            using var scope = services.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var benchmarkOptions = serviceProvider.GetRequiredService<BenchmarkOptions>();
            var agentProfile = configuration.GetSection($"Agents:{agent}").Get<AgentProfile>()
                ?? throw new InvalidOperationException($"Agent '{agent}' is not configured.");
            var catalog = serviceProvider.GetRequiredService<TaskCatalog>();
            var fixtureManager = serviceProvider.GetRequiredService<FixtureManager>();
            var runner = serviceProvider.GetRequiredService<IAgentRunner>();
            var collectors = serviceProvider.GetServices<IMetricCollector>().ToList();
            var repository = serviceProvider.GetRequiredService<RunRepository>();
            await repository.InitializeAsync(CancellationToken.None);

            var loadedTasks = catalog.Load(Path.Combine(repoRoot, benchmarkOptions.TasksDirectory));
            var validationFailures = catalog.Validate(loadedTasks);
            if (validationFailures.Count > 0)
            {
                throw new InvalidOperationException("Task validation failed: " + string.Join("; ", validationFailures.Select(failure => failure.ErrorMessage)));
            }

            var selectedTasks = catalog.Filter(loadedTasks, tasks);
            if (selectedTasks.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No tasks matched the requested filter.[/]");
                return;
            }

            var selectedModel = model ?? PromptForModel(repoRoot, agentProfile.DefaultModel);
            var skillsPath = onlyWithout ? null : ResolveSkillsPath(repoRoot, withSkills, benchmarkOptions.SkillsFilePath, onlyWith);
            var verifiers = new List<IVerifier> { serviceProvider.GetRequiredService<TestRunnerVerifier>() };
            if (!skipJudge)
            {
                verifiers.Add(new LlmJudgeVerifier(
                    serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient(),
                    judgeModel ?? configuration["Judge:Model"] ?? "qwen/qwq-32b",
                    configuration["Judge:ApiKeyEnvironmentVariable"] ?? "VENICE_API_KEY",
                    configuration["Judge:ApiBaseUrl"] ?? "https://api.venice.ai/api/v1",
                    configuration.GetValue("Judge:Repeats", 3)));
            }

            var resultsRoot = Path.Combine(repoRoot, output ?? benchmarkOptions.ResultsDirectory);
            Directory.CreateDirectory(resultsRoot);
            var runId = Guid.NewGuid().ToString("N");
            var run = new BenchmarkRun
            {
                RunId = runId,
                Timestamp = DateTimeOffset.UtcNow,
                AgentName = agent,
                Provider = agentProfile.Provider,
                Model = selectedModel,
                Tool = agentProfile.Executable,
                Status = "running"
            };

            foreach (var taskDefinition in selectedTasks)
            {
                for (var iteration = 0; iteration < Math.Max(1, repeats > 0 ? repeats : benchmarkOptions.DefaultRepeats); iteration++)
                {
                    var taskOutputDirectory = Path.Combine(resultsRoot, runId);
                    var workspacePath = await fixtureManager.PrepareWorkspaceAsync(
                        Path.Combine(repoRoot, benchmarkOptions.FixturesDirectory),
                        taskDefinition,
                        taskOutputDirectory,
                        skillsPath,
                        CancellationToken.None);

                    var timeoutMinutes = timeout ?? (taskDefinition.TimeoutMinutes > 0 ? taskDefinition.TimeoutMinutes : benchmarkOptions.DefaultTimeoutMinutes);
                    var runContext = new RunContext
                    {
                        RunId = runId,
                        RepoRoot = repoRoot,
                        WorkspacePath = workspacePath,
                        Prompt = BuildPrompt(taskDefinition, skillsPath is not null),
                        AgentProfile = agentProfile,
                        Model = selectedModel,
                        Timeout = TimeSpan.FromMinutes(timeoutMinutes),
                        TaskDefinition = taskDefinition,
                        ToolName = agentProfile.Executable
                    };

                    var runResult = await runner.RunAsync(runContext, CancellationToken.None);
                    var taskRun = new TaskRunEntity
                    {
                        BenchmarkRunId = runId,
                        TaskId = taskDefinition.Id,
                        Title = taskDefinition.Title,
                        Status = runResult.Succeeded ? "completed" : "failed",
                        Passed = runResult.Succeeded,
                        WallClockSeconds = runResult.WallClockSeconds,
                        InputTokens = runResult.TokenUsage.InputTokens,
                        OutputTokens = runResult.TokenUsage.OutputTokens,
                        WorkspacePath = workspacePath,
                        Summary = string.IsNullOrWhiteSpace(runResult.ErrorOutput) ? runResult.RawOutput : runResult.ErrorOutput
                    };

                    var metrics = new MetricSet();
                    foreach (var collector in collectors)
                    {
                        metrics.Merge(await collector.CollectAsync(runContext, runResult, CancellationToken.None));
                    }

                    foreach (var metric in metrics.Records)
                    {
                        taskRun.Metrics.Add(new MetricEntity
                        {
                            Name = metric.Name,
                            Value = metric.Value,
                            Unit = metric.Unit ?? string.Empty
                        });
                    }

                    if (decimal.TryParse(taskRun.Metrics.FirstOrDefault(metric => metric.Name == "estimated_cost_usd")?.Value, out var cost))
                    {
                        taskRun.CostUsd = cost;
                    }

                    var verificationResults = new List<VerificationResult>();
                    foreach (var verifier in verifiers)
                    {
                        verificationResults.Add(await verifier.VerifyAsync(runContext, runResult, CancellationToken.None));
                    }

                    var verification = VerificationResult.Combine(verificationResults);
                    taskRun.Passed = verification.Passed;
                    taskRun.Status = verification.Passed ? "passed" : "failed";
                    taskRun.VerificationSummary = verification.Summary;
                    run.TaskRuns.Add(taskRun);
                }
            }

            run.TaskCount = run.TaskRuns.Count;
            run.TotalInputTokens = run.TaskRuns.Sum(taskRun => taskRun.InputTokens);
            run.TotalOutputTokens = run.TaskRuns.Sum(taskRun => taskRun.OutputTokens);
            run.TotalCostUsd = run.TaskRuns.Sum(taskRun => taskRun.CostUsd);
            run.TotalWallClockSeconds = run.TaskRuns.Sum(taskRun => taskRun.WallClockSeconds);
            run.Status = run.TaskRuns.All(taskRun => taskRun.Passed) ? "passed" : "completed";

            await repository.SaveRunAsync(run, CancellationToken.None);

            foreach (var renderer in scope.ServiceProvider.GetServices<IReportRenderer>())
            {
                await renderer.RenderAsync(run, run.TaskRuns, Path.Combine(resultsRoot, runId), CancellationToken.None);
            }

            AnsiConsole.MarkupLine($"[green]Run {run.RunId} completed with {run.TaskRuns.Count} task run(s).[/]");
        });

        return command;
    }

    private static Command BuildReportCommand(IServiceProvider services, string repoRoot)
    {
        var command = new Command("report", "Re-render reports from a stored run.");
        var runIdOption = new Option<string>("--run-id") { IsRequired = true };
        var formatOption = new Option<string>("--format", () => "all");
        command.AddOption(runIdOption);
        command.AddOption(formatOption);
        command.SetHandler(async (string runId, string format) =>
        {
            using var scope = services.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<RunRepository>();
            await repository.InitializeAsync(CancellationToken.None);
            var run = await repository.GetRunAsync(runId, CancellationToken.None) ?? throw new InvalidOperationException($"Run '{runId}' was not found.");
            var renderers = scope.ServiceProvider.GetServices<IReportRenderer>()
                .Where(renderer => string.Equals(format, "all", StringComparison.OrdinalIgnoreCase) || string.Equals(renderer.Format, format, StringComparison.OrdinalIgnoreCase));
            foreach (var renderer in renderers)
            {
                await renderer.RenderAsync(run, run.TaskRuns, Path.Combine(repoRoot, "results", runId), CancellationToken.None);
            }
        }, runIdOption, formatOption);
        return command;
    }

    private static Command BuildListTasksCommand(IServiceProvider services, string repoRoot)
    {
        var command = new Command("list-tasks", "List benchmark tasks.");
        command.SetHandler(() =>
        {
            using var scope = services.CreateScope();
            var options = scope.ServiceProvider.GetRequiredService<BenchmarkOptions>();
            var catalog = scope.ServiceProvider.GetRequiredService<TaskCatalog>();
            var tasks = catalog.Load(Path.Combine(repoRoot, options.TasksDirectory));
            var table = new Table().AddColumns("Id", "Category", "Fixture", "Difficulty", "Timeout");
            foreach (var task in tasks.OrderBy(task => task.Id))
            {
                table.AddRow(task.Id, task.Category.ToString(), task.Fixture, task.Difficulty, task.TimeoutMinutes.ToString());
            }
            AnsiConsole.Write(table);
        });
        return command;
    }

    private static Command BuildValidateTasksCommand(IServiceProvider services, string repoRoot)
    {
        var command = new Command("validate-tasks", "Validate all task JSON files.");
        command.SetHandler(() =>
        {
            using var scope = services.CreateScope();
            var options = scope.ServiceProvider.GetRequiredService<BenchmarkOptions>();
            var catalog = scope.ServiceProvider.GetRequiredService<TaskCatalog>();
            var tasks = catalog.Load(Path.Combine(repoRoot, options.TasksDirectory));
            var failures = catalog.Validate(tasks);
            if (failures.Count == 0)
            {
                AnsiConsole.MarkupLine($"[green]Validated {tasks.Count} task(s) successfully.[/]");
                return;
            }

            foreach (var failure in failures)
            {
                AnsiConsole.MarkupLine($"[red]{failure.ErrorMessage}[/]");
            }

            throw new InvalidOperationException("Task validation failed.");
        });
        return command;
    }

    private static Command BuildInitSkillsCommand(string repoRoot)
    {
        var command = new Command("init-skills", "Fetch the canonical AGENTS.md skills file.");
        command.SetHandler(async () =>
        {
            using var client = new HttpClient();
            var content = await client.GetStringAsync("https://raw.githubusercontent.com/multica-ai/andrej-karpathy-skills/main/AGENTS.md");
            var skillsDirectory = Path.Combine(repoRoot, "skills");
            Directory.CreateDirectory(skillsDirectory);
            await File.WriteAllTextAsync(Path.Combine(skillsDirectory, "AGENTS.md"), content);
            AnsiConsole.MarkupLine("[green]skills/AGENTS.md updated.[/]");
        });
        return command;
    }

    private static Command BuildHistoryCommand(IServiceProvider services)
    {
        var command = new Command("history", "Show benchmark run history.");
        command.SetHandler(async () =>
        {
            using var scope = services.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<RunRepository>();
            await repository.InitializeAsync(CancellationToken.None);
            var runs = await repository.GetHistoryAsync(20, CancellationToken.None);
            var table = new Table().AddColumns("RunId", "Provider", "Tool", "Model", "Tokens", "Seconds", "Cost", "Status");
            foreach (var run in runs)
            {
                table.AddRow(
                    run.RunId,
                    run.Provider,
                    run.Tool,
                    run.Model,
                    (run.TotalInputTokens + run.TotalOutputTokens).ToString(),
                    run.TotalWallClockSeconds.ToString("0.##"),
                    run.TotalCostUsd.ToString("0.0000"),
                    run.Status);
            }
            AnsiConsole.Write(table);
        });
        return command;
    }

    private static string BuildPrompt(TaskDefinition taskDefinition, bool usingSkills)
    {
        var header = usingSkills
            ? "Use the workspace AGENTS.md skills guidance while solving this task."
            : "Solve the task without relying on an AGENTS.md skills file.";
        var expectedBehavior = string.Join(Environment.NewLine, taskDefinition.ExpectedBehavior.Select(item => $"- {item}"));
        return $"""
{header}
Task: {taskDefinition.Title}
Prompt:
{taskDefinition.Prompt}

Expected behavior:
{expectedBehavior}
""";
    }

    private static string? ResolveSkillsPath(string repoRoot, string? withSkills, string configuredPath, bool required)
    {
        var path = withSkills is not null
            ? Path.GetFullPath(withSkills, repoRoot)
            : Path.Combine(repoRoot, configuredPath.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(path))
        {
            if (required)
            {
                throw new FileNotFoundException($"Skills file '{path}' was not found.");
            }

            return null;
        }

        return path;
    }

    private static string PromptForModel(string repoRoot, string fallback)
    {
        var configPath = Path.Combine(repoRoot, "opencode.json");
        if (!File.Exists(configPath) || Console.IsInputRedirected)
        {
            return fallback;
        }

        using var document = JsonDocument.Parse(File.ReadAllText(configPath));
        var models = document.RootElement.GetProperty("provider").GetProperty("venice").GetProperty("models")
            .EnumerateObject()
            .Select(property => new ModelOption(property.Name, property.Value.GetProperty("name").GetString() ?? property.Name))
            .ToList();
        var labels = models.ToDictionary(model => $"{model.Name} ({model.Id})", model => model.Id);
        var choice = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title("Select a Venice AI model")
            .AddChoices(labels.Keys));
        return labels[choice];
    }

    private sealed record ModelOption(string Id, string Name);
}
