namespace KarpathySkillsBenchmark.Runners;

public sealed class ToolCallRecord
{
    public string Name { get; set; } = string.Empty;

    public DateTimeOffset TimestampUtc { get; set; } = DateTimeOffset.UtcNow;

    public string Details { get; set; } = string.Empty;
}
