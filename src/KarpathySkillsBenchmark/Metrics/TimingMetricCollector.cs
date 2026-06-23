using KarpathySkillsBenchmark.Runners;

namespace KarpathySkillsBenchmark.Metrics;

public sealed class TimingMetricCollector : IMetricCollector
{
    public string Name => "timing";

    public Task<MetricSet> CollectAsync(RunContext context, AgentRunResult result, CancellationToken cancellationToken)
        => Task.FromResult(new MetricSet().Add("wall_clock_seconds", result.WallClockSeconds, "seconds"));
}
