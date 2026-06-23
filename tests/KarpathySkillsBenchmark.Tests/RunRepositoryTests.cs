using KarpathySkillsBenchmark.Storage;
using KarpathySkillsBenchmark.Storage.Entities;
using Microsoft.EntityFrameworkCore;

namespace KarpathySkillsBenchmark.Tests;

public sealed class RunRepositoryTests
{
    [Fact]
    public async Task SaveAndLoad_WorksWithSqlite()
    {
        var root = TestWorkspace.Create(nameof(SaveAndLoad_WorksWithSqlite));
        var connectionString = $"Data Source={Path.Combine(root, "benchmark.db")}";
        var options = new DbContextOptionsBuilder<BenchmarkDbContext>()
            .UseSqlite(connectionString)
            .Options;

        await using var context = new BenchmarkDbContext(options);
        var repository = new RunRepository(context);
        await repository.InitializeAsync(CancellationToken.None);

        var run = new BenchmarkRun
        {
            RunId = "run-1",
            AgentName = "opencode",
            Provider = "venice",
            Model = "model",
            Tool = "opencode",
            Status = "passed",
            TaskCount = 1,
            TaskRuns = [new TaskRunEntity { TaskId = "task-1", Title = "task", Status = "passed", Passed = true, Metrics = [new MetricEntity { Name = "m", Value = "1" }] }]
        };

        await repository.SaveRunAsync(run, CancellationToken.None);
        var loaded = await repository.GetRunAsync("run-1", CancellationToken.None);

        Assert.NotNull(loaded);
        Assert.Single(loaded!.TaskRuns);
        Assert.Single(loaded.TaskRuns[0].Metrics);
    }
}
