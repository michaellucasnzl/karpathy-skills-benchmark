using KarpathySkillsBenchmark.Runners;

namespace KarpathySkillsBenchmark.Metrics;

public sealed class BehaviorMetricCollector : IMetricCollector
{
    public string Name => "behavior";

    public Task<MetricSet> CollectAsync(RunContext context, AgentRunResult result, CancellationToken cancellationToken)
        => Task.FromResult(new MetricSet()
            .Add("exit_code", result.ExitCode)
            .Add("succeeded", result.Succeeded ? 1 : 0)
            .Add("output_characters", result.RawOutput.Length));
}
