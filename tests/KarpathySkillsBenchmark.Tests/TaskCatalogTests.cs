using FluentValidation;
using KarpathySkillsBenchmark.Tasks;

namespace KarpathySkillsBenchmark.Tests;

public sealed class TaskCatalogTests
{
    [Fact]
    public void LoadAndFilter_ReturnsExpectedTasks()
    {
        var root = TestWorkspace.Create(nameof(LoadAndFilter_ReturnsExpectedTasks));
        var tasksDir = Path.Combine(root, "tasks", "bug-fixes");
        Directory.CreateDirectory(tasksDir);

        var definition = new TaskDefinition
        {
            Id = "bugfix-sample-01",
            Category = TaskCategory.BugFixes,
            Title = "Sample",
            Difficulty = "easy",
            Fixture = "fixture-a",
            Prompt = "Fix it",
            ExpectedBehavior = ["Works"],
            SuccessCriteria = [new SuccessCriterion { Type = "test", Command = "dotnet test", ExpectPass = true }],
            EstimatedLinesChanged = 1,
            TimeoutMinutes = 5
        };

        File.WriteAllText(Path.Combine(tasksDir, "sample.json"), System.Text.Json.JsonSerializer.Serialize(definition, TaskCatalog.SerializerOptions));

        var catalog = new TaskCatalog(new TaskDefinitionValidator());
        var loaded = catalog.Load(Path.Combine(root, "tasks"));
        var filtered = catalog.Filter(loaded, "bugfix-*");

        Assert.Single(loaded);
        Assert.Single(filtered);
        Assert.Equal("bugfix-sample-01", filtered[0].Id);
    }
}
