using KarpathySkillsBenchmark.Runners;

namespace KarpathySkillsBenchmark.Metrics;

public interface IMetricCollector
{
    string Name { get; }

    Task<MetricSet> CollectAsync(RunContext context, AgentRunResult result, CancellationToken cancellationToken);
}
