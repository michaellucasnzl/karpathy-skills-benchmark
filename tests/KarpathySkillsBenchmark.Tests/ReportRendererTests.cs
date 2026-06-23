using KarpathySkillsBenchmark.Reporting;
using KarpathySkillsBenchmark.Storage.Entities;

namespace KarpathySkillsBenchmark.Tests;

public sealed class ReportRendererTests
{
    [Fact]
    public async Task Renderers_CreateOutputFiles()
    {
        var output = TestWorkspace.Create(nameof(Renderers_CreateOutputFiles));
        var run = new BenchmarkRun
        {
            RunId = "run-1",
            AgentName = "opencode",
            Provider = "venice",
            Model = "venice/model",
            TaskCount = 1,
            TotalCostUsd = 1.23m
        };
        var tasks = new[] { new TaskRunEntity { TaskId = "task-1", Status = "passed", InputTokens = 1, OutputTokens = 2, WallClockSeconds = 3, CostUsd = 0.5m } };

        var markdown = await new MarkdownReportRenderer().RenderAsync(run, tasks, output, CancellationToken.None);
        var html = await new HtmlReportRenderer().RenderAsync(run, tasks, output, CancellationToken.None);
        var csv = await new CsvReportRenderer().RenderAsync(run, tasks, output, CancellationToken.None);

        Assert.True(File.Exists(markdown));
        Assert.True(File.Exists(html));
        Assert.True(File.Exists(csv));
    }
}
