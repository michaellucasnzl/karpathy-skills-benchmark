using System.Globalization;
using System.Text;
using KarpathySkillsBenchmark.Storage.Entities;

namespace KarpathySkillsBenchmark.Reporting;

public sealed class CsvReportRenderer : IReportRenderer
{
    public string Format => "csv";

    public async Task<string> RenderAsync(BenchmarkRun run, IReadOnlyCollection<TaskRunEntity> taskRuns, string outputDirectory, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(outputDirectory);
        var path = Path.Combine(outputDirectory, $"{run.RunId}.csv");
        var builder = new StringBuilder().AppendLine("taskId,status,inputTokens,outputTokens,wallClockSeconds,costUsd");
        foreach (var task in taskRuns.OrderBy(task => task.TaskId))
        {
            builder.AppendLine(string.Join(',', new[]
            {
                task.TaskId,
                task.Status,
                task.InputTokens.ToString(CultureInfo.InvariantCulture),
                task.OutputTokens.ToString(CultureInfo.InvariantCulture),
                task.WallClockSeconds.ToString(CultureInfo.InvariantCulture),
                task.CostUsd.ToString(CultureInfo.InvariantCulture)
            }));
        }

        await File.WriteAllTextAsync(path, builder.ToString(), cancellationToken);
        return path;
    }
}
