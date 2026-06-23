using KarpathySkillsBenchmark.Storage.Entities;

namespace KarpathySkillsBenchmark.Reporting;

public interface IReportRenderer
{
    string Format { get; }

    Task<string> RenderAsync(BenchmarkRun run, IReadOnlyCollection<TaskRunEntity> taskRuns, string outputDirectory, CancellationToken cancellationToken);
}
