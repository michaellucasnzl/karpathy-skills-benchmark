namespace KarpathySkillsBenchmark.Storage.Entities;

public sealed class MetricEntity
{
    public int Id { get; set; }

    public int TaskRunEntityId { get; set; }

    public TaskRunEntity? TaskRun { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    public string Unit { get; set; } = string.Empty;
}
