namespace KarpathySkillsBenchmark.Runners;

public sealed class TokenUsage
{
    public int InputTokens { get; set; }

    public int OutputTokens { get; set; }

    public int TotalTokens => InputTokens + OutputTokens;
}
