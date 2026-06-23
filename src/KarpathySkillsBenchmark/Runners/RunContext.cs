using KarpathySkillsBenchmark.Configuration;
using KarpathySkillsBenchmark.Tasks;

namespace KarpathySkillsBenchmark.Runners;

public sealed class RunContext
{
    public string RunId { get; init; } = Guid.NewGuid().ToString("N");

    public TaskDefinition TaskDefinition { get; init; } = new();

    public string WorkspacePath { get; init; } = string.Empty;

    public string Prompt { get; init; } = string.Empty;

    public AgentProfile AgentProfile { get; init; } = new();

    public string Model { get; init; } = string.Empty;

    public TimeSpan Timeout { get; init; } = TimeSpan.FromMinutes(15);

    public string RepoRoot { get; init; } = string.Empty;

    public string ToolName { get; init; } = "opencode";
}
