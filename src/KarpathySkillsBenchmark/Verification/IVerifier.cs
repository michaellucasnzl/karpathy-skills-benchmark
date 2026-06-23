using KarpathySkillsBenchmark.Runners;

namespace KarpathySkillsBenchmark.Verification;

public interface IVerifier
{
    string Name { get; }

    Task<VerificationResult> VerifyAsync(RunContext context, AgentRunResult result, CancellationToken cancellationToken);
}
