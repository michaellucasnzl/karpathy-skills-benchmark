using KarpathySkillsBenchmark.Runners;

namespace KarpathySkillsBenchmark.Tests;

internal sealed class FakeAgentRunner : IAgentRunner
{
    private readonly AgentRunResult _result;

    public FakeAgentRunner(AgentRunResult? result = null)
    {
        _result = result ?? new AgentRunResult
        {
            Succeeded = true,
            ExitCode = 0,
            RawOutput = "fake output",
            StartedAtUtc = DateTimeOffset.UtcNow,
            FinishedAtUtc = DateTimeOffset.UtcNow.AddSeconds(2),
            TokenUsage = new TokenUsage { InputTokens = 12, OutputTokens = 8 },
            ToolCalls = [new ToolCallRecord { Name = "apply_patch", Details = "{}" }]
        };
    }

    public Task<AgentRunResult> RunAsync(RunContext context, CancellationToken cancellationToken)
        => Task.FromResult(_result);
}
