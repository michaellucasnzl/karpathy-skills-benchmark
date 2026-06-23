using System.Net;
using KarpathySkillsBenchmark.Storage.Entities;

namespace KarpathySkillsBenchmark.Reporting;

public sealed class HtmlReportRenderer : IReportRenderer
{
    public string Format => "html";

    public async Task<string> RenderAsync(BenchmarkRun run, IReadOnlyCollection<TaskRunEntity> taskRuns, string outputDirectory, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(outputDirectory);
        var path = Path.Combine(outputDirectory, $"{run.RunId}.html");
        var rows = string.Join(Environment.NewLine, taskRuns.OrderBy(task => task.TaskId).Select(task =>
            $"<tr><td>{WebUtility.HtmlEncode(task.TaskId)}</td><td>{WebUtility.HtmlEncode(task.Status)}</td><td>{task.InputTokens + task.OutputTokens}</td><td>{task.WallClockSeconds:0.##}</td></tr>"));
        var html = $"""
<!DOCTYPE html>
<html lang="en">
<head><meta charset="utf-8"><title>Benchmark {WebUtility.HtmlEncode(run.RunId)}</title></head>
<body>
<h1>Benchmark Run {WebUtility.HtmlEncode(run.RunId)}</h1>
<ul>
<li>Agent: {WebUtility.HtmlEncode(run.AgentName)}</li>
<li>Provider: {WebUtility.HtmlEncode(run.Provider)}</li>
<li>Model: {WebUtility.HtmlEncode(run.Model)}</li>
<li>Tasks: {run.TaskCount}</li>
</ul>
<table border="1" cellpadding="4" cellspacing="0">
<thead><tr><th>Task</th><th>Status</th><th>Tokens</th><th>Seconds</th></tr></thead>
<tbody>
{rows}
</tbody>
</table>
</body>
</html>
""";
        await File.WriteAllTextAsync(path, html, cancellationToken);
        return path;
    }
}
