namespace KarpathySkillsBenchmark.Storage.Entities;

public sealed class TaskRunEntity
{
    public int Id { get; set; }

    public string BenchmarkRunId { get; set; } = string.Empty;

    public BenchmarkRun? BenchmarkRun { get; set; }

    public string TaskId { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public bool Passed { get; set; }

    public double WallClockSeconds { get; set; }

    public int InputTokens { get; set; }

    public int OutputTokens { get; set; }

    public decimal CostUsd { get; set; }

    public string Summary { get; set; } = string.Empty;

    public string VerificationSummary { get; set; } = string.Empty;

    public string WorkspacePath { get; set; } = string.Empty;

    public List<MetricEntity> Metrics { get; set; } = [];
}
