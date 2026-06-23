using System.Globalization;

namespace KarpathySkillsBenchmark.Metrics;

public sealed class MetricSet
{
    private readonly List<MetricRecord> _records = [];

    public IReadOnlyList<MetricRecord> Records => _records;

    public MetricSet Add(string name, string value, string? unit = null)
    {
        _records.Add(new MetricRecord(name, value, unit));
        return this;
    }

    public MetricSet Add(string name, int value, string? unit = null) => Add(name, value.ToString(CultureInfo.InvariantCulture), unit);

    public MetricSet Add(string name, double value, string? unit = null) => Add(name, value.ToString("0.###", CultureInfo.InvariantCulture), unit);

    public MetricSet Add(string name, decimal value, string? unit = null) => Add(name, value.ToString(CultureInfo.InvariantCulture), unit);

    public MetricSet Merge(MetricSet other)
    {
        _records.AddRange(other.Records);
        return this;
    }
}

public sealed record MetricRecord(string Name, string Value, string? Unit = null);

public sealed record PricingRate(decimal InputPer1k, decimal OutputPer1k)
{
    public decimal CalculateCost(int inputTokens, int outputTokens)
        => (inputTokens / 1000m * InputPer1k) + (outputTokens / 1000m * OutputPer1k);
}
