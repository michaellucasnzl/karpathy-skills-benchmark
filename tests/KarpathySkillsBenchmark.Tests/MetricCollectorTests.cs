using KarpathySkillsBenchmark.Configuration;
using KarpathySkillsBenchmark.Metrics;
using KarpathySkillsBenchmark.Runners;
using KarpathySkillsBenchmark.Tasks;
using LibGit2Sharp;

namespace KarpathySkillsBenchmark.Tests;

public sealed class MetricCollectorTests
{
    [Fact]
    public async Task Collectors_ReturnExpectedMetrics()
    {
        var workspace = TestWorkspace.Create(nameof(Collectors_ReturnExpectedMetrics));
        File.WriteAllText(Path.Combine(workspace, "Sample.cs"), "public class Sample { public int Add(int a, int b) => a + b; }");
        Repository.Init(workspace);
        using (var repo = new Repository(workspace))
        {
            Commands.Stage(repo, "*");
            var author = new Signature("Test", "test@example.com", DateTimeOffset.UtcNow);
            repo.Commit("initial", author, author);
        }
        File.AppendAllText(Path.Combine(workspace, "Sample.cs"), "\npublic int Sub(int a, int b) => a - b;");

        var context = new RunContext
        {
            WorkspacePath = workspace,
            Prompt = "Do the thing",
            Model = "venice/qwen/qwq-32b",
            AgentProfile = new AgentProfile { Provider = "venice" },
            TaskDefinition = new TaskDefinition { Id = "task", Title = "Task", Difficulty = "easy", Fixture = "fixture", Prompt = "p", SuccessCriteria = [new SuccessCriterion { Type = "test", Command = "dotnet test" }] }
        };
        var result = new AgentRunResult
        {
            Succeeded = true,
            ExitCode = 0,
            RawOutput = "output",
            StartedAtUtc = DateTimeOffset.UtcNow,
            FinishedAtUtc = DateTimeOffset.UtcNow.AddSeconds(1),
            TokenUsage = new TokenUsage { InputTokens = 100, OutputTokens = 50 },
            ToolCalls = [new ToolCallRecord { Name = "edit" }]
        };

        var diff = await new DiffMetricCollector().CollectAsync(context, result, CancellationToken.None);
        var token = await new TokenMetricCollector(new Dictionary<string, PricingRate> { ["venice/qwen/qwq-32b"] = new(0.001m, 0.002m) }).CollectAsync(context, result, CancellationToken.None);
        var timing = await new TimingMetricCollector().CollectAsync(context, result, CancellationToken.None);
        var complexity = await new ComplexityMetricCollector().CollectAsync(context, result, CancellationToken.None);
        var behavior = await new BehaviorMetricCollector().CollectAsync(context, result, CancellationToken.None);

        Assert.Contains(diff.Records, metric => metric.Name == "changed_files");
        Assert.Contains(token.Records, metric => metric.Name == "estimated_cost_usd");
        Assert.Contains(timing.Records, metric => metric.Name == "wall_clock_seconds");
        Assert.Contains(complexity.Records, metric => metric.Name == "syntax_nodes");
        Assert.Contains(behavior.Records, metric => metric.Name == "succeeded" && metric.Value == "1");
    }
}
