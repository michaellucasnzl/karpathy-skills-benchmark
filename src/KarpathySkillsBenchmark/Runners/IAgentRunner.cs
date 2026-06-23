namespace KarpathySkillsBenchmark.Runners;

public interface IAgentRunner
{
    Task<AgentRunResult> RunAsync(RunContext context, CancellationToken cancellationToken);
}
