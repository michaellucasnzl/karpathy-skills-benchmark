namespace KarpathySkillsBenchmark.Comparison;

public sealed class ComparisonReport
{
    public string BaselineRunId { get; init; } = string.Empty;

    public string CandidateRunId { get; init; } = string.Empty;

    public int PassedTaskDelta { get; init; }

    public int InputTokenDelta { get; init; }

    public int OutputTokenDelta { get; init; }

    public decimal CostDeltaUsd { get; init; }

    public double DurationDeltaSeconds { get; init; }
}
