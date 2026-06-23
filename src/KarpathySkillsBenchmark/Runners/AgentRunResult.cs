namespace KarpathySkillsBenchmark.Runners;

public sealed class AgentRunResult
{
    public bool Succeeded { get; set; }

    public int ExitCode { get; set; }

    public string RawOutput { get; set; } = string.Empty;

    public string ErrorOutput { get; set; } = string.Empty;

    public DateTimeOffset StartedAtUtc { get; set; }

    public DateTimeOffset FinishedAtUtc { get; set; }

    public TokenUsage TokenUsage { get; set; } = new();

    public List<ToolCallRecord> ToolCalls { get; set; } = [];

    public double WallClockSeconds => Math.Max(0d, (FinishedAtUtc - StartedAtUtc).TotalSeconds);
}
