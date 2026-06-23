using KarpathySkillsBenchmark.Storage.Entities;

namespace KarpathySkillsBenchmark.Comparison;

public sealed class RunComparator
{
    public ComparisonReport Compare(BenchmarkRun baseline, BenchmarkRun candidate)
    {
        var baselinePassed = baseline.TaskRuns.Count(run => run.Passed);
        var candidatePassed = candidate.TaskRuns.Count(run => run.Passed);

        return new ComparisonReport
        {
            BaselineRunId = baseline.RunId,
            CandidateRunId = candidate.RunId,
            PassedTaskDelta = candidatePassed - baselinePassed,
            InputTokenDelta = candidate.TotalInputTokens - baseline.TotalInputTokens,
            OutputTokenDelta = candidate.TotalOutputTokens - baseline.TotalOutputTokens,
            CostDeltaUsd = candidate.TotalCostUsd - baseline.TotalCostUsd,
            DurationDeltaSeconds = candidate.TotalWallClockSeconds - baseline.TotalWallClockSeconds
        };
    }
}
