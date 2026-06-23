namespace KarpathySkillsBenchmark.Verification;

public sealed class VerificationResult
{
    public bool Passed { get; init; }

    public double Score { get; init; }

    public string Summary { get; init; } = string.Empty;

    public List<VerificationStepResult> Steps { get; init; } = [];

    public static VerificationResult Combine(params IEnumerable<VerificationResult>[] resultGroups)
    {
        var flattened = resultGroups.SelectMany(group => group).ToList();
        return new VerificationResult
        {
            Passed = flattened.All(result => result.Passed),
            Score = flattened.Count == 0 ? 0 : flattened.Average(result => result.Score),
            Summary = string.Join(Environment.NewLine, flattened.Where(result => !string.IsNullOrWhiteSpace(result.Summary)).Select(result => result.Summary)),
            Steps = flattened.SelectMany(result => result.Steps).ToList()
        };
    }
}

public sealed class VerificationStepResult
{
    public string Name { get; init; } = string.Empty;

    public bool Passed { get; init; }

    public string Summary { get; init; } = string.Empty;

    public double? Score { get; init; }
}
