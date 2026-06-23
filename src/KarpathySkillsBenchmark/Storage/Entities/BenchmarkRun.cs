namespace KarpathySkillsBenchmark.Storage.Entities;

public sealed class BenchmarkRun
{
    public string RunId { get; set; } = Guid.NewGuid().ToString("N");

    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    public string AgentName { get; set; } = string.Empty;

    public string Provider { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public string Tool { get; set; } = string.Empty;

    public int TaskCount { get; set; }

    public int TotalInputTokens { get; set; }

    public int TotalOutputTokens { get; set; }

    public decimal TotalCostUsd { get; set; }

    public double TotalWallClockSeconds { get; set; }

    public string Status { get; set; } = string.Empty;

    public List<TaskRunEntity> TaskRuns { get; set; } = [];
}
