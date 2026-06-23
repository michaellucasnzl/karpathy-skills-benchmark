using KarpathySkillsBenchmark.Runners;

namespace KarpathySkillsBenchmark.Metrics;

public sealed class TokenMetricCollector : IMetricCollector
{
    private readonly IReadOnlyDictionary<string, PricingRate> _pricing;

    public TokenMetricCollector(IReadOnlyDictionary<string, PricingRate> pricing)
    {
        _pricing = pricing;
    }

    public string Name => "tokens";

    public Task<MetricSet> CollectAsync(RunContext context, AgentRunResult result, CancellationToken cancellationToken)
    {
        var usage = result.TokenUsage;
        var metricSet = new MetricSet()
            .Add("input_tokens", usage.InputTokens)
            .Add("output_tokens", usage.OutputTokens)
            .Add("total_tokens", usage.TotalTokens);

        if (_pricing.TryGetValue(context.Model, out var rate) || _pricing.TryGetValue($"{context.AgentProfile.Provider}/{context.Model.Split('/').Last()}", out rate))
        {
            metricSet.Add("estimated_cost_usd", rate.CalculateCost(usage.InputTokens, usage.OutputTokens), "usd");
        }
        else
        {
            metricSet.Add("estimated_cost_usd", 0m, "usd");
        }

        return Task.FromResult(metricSet);
    }
}
