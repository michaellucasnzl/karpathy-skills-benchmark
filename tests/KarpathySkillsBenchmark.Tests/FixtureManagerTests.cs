using KarpathySkillsBenchmark.Fixtures;
using KarpathySkillsBenchmark.Tasks;
using LibGit2Sharp;

namespace KarpathySkillsBenchmark.Tests;

public sealed class FixtureManagerTests
{
    [Fact]
    public async Task PrepareWorkspace_CopiesFixtureAndSkillsFile()
    {
        var root = TestWorkspace.Create(nameof(PrepareWorkspace_CopiesFixtureAndSkillsFile));
        var fixturesDir = Path.Combine(root, "fixtures");
        var fixturePath = Path.Combine(fixturesDir, "fixture-a");
        Directory.CreateDirectory(fixturePath);
        File.WriteAllText(Path.Combine(fixturePath, "sample.txt"), "hello");
        Repository.Init(fixturePath);
        using (var repo = new Repository(fixturePath))
        {
            Commands.Stage(repo, "*");
            var author = new Signature("Test", "test@example.com", DateTimeOffset.UtcNow);
            repo.Commit("initial", author, author);
        }

        var skillsPath = Path.Combine(root, "skills.md");
        File.WriteAllText(skillsPath, "skills");

        var manager = new FixtureManager();
        var workspace = await manager.PrepareWorkspaceAsync(fixturesDir, new TaskDefinition { Fixture = "fixture-a", StartingCommit = "master" }, Path.Combine(root, "output"), skillsPath, CancellationToken.None);

        Assert.True(File.Exists(Path.Combine(workspace, "sample.txt")));
        Assert.True(File.Exists(Path.Combine(workspace, "AGENTS.md")));
    }
}
