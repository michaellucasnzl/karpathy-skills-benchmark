using KarpathySkillsBenchmark.Configuration;
using KarpathySkillsBenchmark.Metrics;
using KarpathySkillsBenchmark.Reporting;
using KarpathySkillsBenchmark.Runners;
using KarpathySkillsBenchmark.Storage.Entities;
using KarpathySkillsBenchmark.Tasks;
using KarpathySkillsBenchmark.Verification;

namespace KarpathySkillsBenchmark.Tests;

public sealed class EndToEndTests
{
    [Fact]
    public async Task EndToEnd_FakeRunner_FullBenchmarkRun_ProducesReport()
    {
        var workspace = TestWorkspace.Create(nameof(EndToEnd_FakeRunner_FullBenchmarkRun_ProducesReport));
        File.WriteAllText(Path.Combine(workspace, "Sample.cs"), "public class Sample { } ");

        var taskDefinition = new TaskDefinition
        {
            Id = "task-1",
            Category = TaskCategory.Trivial,
            Title = "Task",
            Difficulty = "easy",
            Fixture = "fixture",
            Prompt = "Do it",
            ExpectedBehavior = ["Done"],
            SuccessCriteria = []
        };
        var context = new RunContext
        {
            WorkspacePath = workspace,
            RepoRoot = workspace,
            Prompt = "Do it",
            Model = "venice/qwen/qwq-32b",
            AgentProfile = new AgentProfile { Provider = "venice" },
            TaskDefinition = taskDefinition
        };

        var runner = new FakeAgentRunner();
        var result = await runner.RunAsync(context, CancellationToken.None);
        var tokenMetrics = await new TokenMetricCollector(new Dictionary<string, PricingRate> { ["venice/qwen/qwq-32b"] = new(0.001m, 0.002m) }).CollectAsync(context, result, CancellationToken.None);
        var verification = VerificationResult.Combine(Array.Empty<VerificationResult>());

        var taskRun = new TaskRunEntity
        {
            TaskId = taskDefinition.Id,
            Title = taskDefinition.Title,
            Status = "passed",
            Passed = true,
            InputTokens = result.TokenUsage.InputTokens,
            OutputTokens = result.TokenUsage.OutputTokens,
            WallClockSeconds = result.WallClockSeconds,
            CostUsd = decimal.Parse(tokenMetrics.Records.First(record => record.Name == "estimated_cost_usd").Value)
        };

        var run = new BenchmarkRun
        {
            RunId = "run-1",
            AgentName = "opencode",
            Provider = "venice",
            Model = context.Model,
            Tool = "opencode",
            TaskCount = 1,
            TotalInputTokens = taskRun.InputTokens,
            TotalOutputTokens = taskRun.OutputTokens,
            TotalCostUsd = taskRun.CostUsd,
            TotalWallClockSeconds = taskRun.WallClockSeconds,
            Status = verification.Passed ? "passed" : "failed",
            TaskRuns = [taskRun]
        };

        var renderer = new MarkdownReportRenderer();
        var report = await renderer.RenderAsync(run, run.TaskRuns, workspace, CancellationToken.None);

        Assert.True(File.Exists(report));
        Assert.Contains("Benchmark Run run-1", await File.ReadAllTextAsync(report));
    }
}
