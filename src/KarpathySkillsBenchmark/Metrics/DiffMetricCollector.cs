using LibGit2Sharp;
using KarpathySkillsBenchmark.Runners;

namespace KarpathySkillsBenchmark.Metrics;

public sealed class DiffMetricCollector : IMetricCollector
{
    public string Name => "diff";

    public Task<MetricSet> CollectAsync(RunContext context, AgentRunResult result, CancellationToken cancellationToken)
    {
        var metrics = new MetricSet();
        if (!Repository.IsValid(context.WorkspacePath))
        {
            return Task.FromResult(metrics.Add("changed_files", 0).Add("lines_added", 0).Add("lines_deleted", 0));
        }

        using var repository = new Repository(context.WorkspacePath);
        var patch = repository.Diff.Compare<Patch>(repository.Head.Tip.Tree, DiffTargets.WorkingDirectory);
        var added = patch.Sum(change => change.LinesAdded);
        var deleted = patch.Sum(change => change.LinesDeleted);

        return Task.FromResult(metrics
            .Add("changed_files", patch.Count())
            .Add("lines_added", added)
            .Add("lines_deleted", deleted));
    }
}
