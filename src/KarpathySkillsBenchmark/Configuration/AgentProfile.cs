namespace KarpathySkillsBenchmark.Configuration;

public sealed class AgentProfile
{
    public string Executable { get; set; } = string.Empty;

    public string Arguments { get; set; } = string.Empty;

    public string Provider { get; set; } = string.Empty;

    public string DefaultModel { get; set; } = string.Empty;
}
