using KarpathySkillsBenchmark.Comparison;
using KarpathySkillsBenchmark.Storage.Entities;

namespace KarpathySkillsBenchmark.Tests;

public sealed class RunComparatorTests
{
    [Fact]
    public void Compare_ComputesDeltas()
    {
        var comparator = new RunComparator();
        var baseline = new BenchmarkRun
        {
            RunId = "base",
            TotalInputTokens = 10,
            TotalOutputTokens = 5,
            TotalCostUsd = 1.5m,
            TotalWallClockSeconds = 10,
            TaskRuns = [new TaskRunEntity { Passed = true }, new TaskRunEntity { Passed = false }]
        };
        var candidate = new BenchmarkRun
        {
            RunId = "cand",
            TotalInputTokens = 15,
            TotalOutputTokens = 6,
            TotalCostUsd = 2m,
            TotalWallClockSeconds = 8,
            TaskRuns = [new TaskRunEntity { Passed = true }, new TaskRunEntity { Passed = true }]
        };

        var report = comparator.Compare(baseline, candidate);
        Assert.Equal(1, report.PassedTaskDelta);
        Assert.Equal(5, report.InputTokenDelta);
        Assert.Equal(-2, report.DurationDeltaSeconds);
    }
}
