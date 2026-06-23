using System.Globalization;
using System.Text;
using KarpathySkillsBenchmark.Storage.Entities;

namespace KarpathySkillsBenchmark.Reporting;

public sealed class MarkdownReportRenderer : IReportRenderer
{
    public string Format => "md";

    public async Task<string> RenderAsync(BenchmarkRun run, IReadOnlyCollection<TaskRunEntity> taskRuns, string outputDirectory, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(outputDirectory);
        var path = Path.Combine(outputDirectory, $"{run.RunId}.md");
        var builder = new StringBuilder()
            .AppendLine($"# Benchmark Run {run.RunId}")
            .AppendLine()
            .AppendLine($"- Agent: {run.AgentName}")
            .AppendLine($"- Provider: {run.Provider}")
            .AppendLine($"- Model: {run.Model}")
            .AppendLine($"- Tasks: {run.TaskCount}")
            .AppendLine($"- Cost (USD): {run.TotalCostUsd.ToString(CultureInfo.InvariantCulture)}")
            .AppendLine()
            .AppendLine("| Task | Status | Tokens | Seconds |")
            .AppendLine("| --- | --- | ---: | ---: |");

        foreach (var task in taskRuns.OrderBy(task => task.TaskId))
        {
            builder.AppendLine($"| {task.TaskId} | {task.Status} | {task.InputTokens + task.OutputTokens} | {task.WallClockSeconds:0.##} |");
        }

        await File.WriteAllTextAsync(path, builder.ToString(), cancellationToken);
        return path;
    }
}
