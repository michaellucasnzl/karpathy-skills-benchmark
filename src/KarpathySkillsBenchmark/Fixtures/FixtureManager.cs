using LibGit2Sharp;
using KarpathySkillsBenchmark.Tasks;

namespace KarpathySkillsBenchmark.Fixtures;

public sealed class FixtureManager
{
    public async Task<string> PrepareWorkspaceAsync(
        string fixturesDirectory,
        TaskDefinition task,
        string outputDirectory,
        string? skillsFilePath,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var sourceFixturePath = Path.Combine(fixturesDirectory, task.Fixture);
        if (!Directory.Exists(sourceFixturePath))
        {
            throw new DirectoryNotFoundException($"Fixture '{task.Fixture}' was not found in '{fixturesDirectory}'.");
        }

        var workspacePath = Path.Combine(outputDirectory, task.Id + "-" + Guid.NewGuid().ToString("N"));
        CopyDirectory(sourceFixturePath, workspacePath);

        if (!string.IsNullOrWhiteSpace(task.StartingCommit) && Repository.IsValid(workspacePath))
        {
            using var repository = new Repository(workspacePath);
            var branch = repository.Branches[task.StartingCommit];
            if (branch is not null)
            {
                Commands.Checkout(repository, branch);
            }
            else
            {
                var commit = repository.Lookup<Commit>(task.StartingCommit);
                if (commit is not null)
                {
                    Commands.Checkout(repository, commit);
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(skillsFilePath) && File.Exists(skillsFilePath))
        {
            File.Copy(skillsFilePath, Path.Combine(workspacePath, "AGENTS.md"), overwrite: true);
        }

        await Task.CompletedTask;
        return workspacePath;
    }

    public string GetFixturePath(string fixturesDirectory, string fixtureName) => Path.Combine(fixturesDirectory, fixtureName);

    public void CopyOpenCodeConfig(string repoRoot, string workspacePath)
    {
        var sourcePath = Path.Combine(repoRoot, "opencode.json");
        if (File.Exists(sourcePath))
        {
            File.Copy(sourcePath, Path.Combine(workspacePath, "opencode.json"), overwrite: true);
        }
    }

    private static void CopyDirectory(string sourcePath, string destinationPath)
    {
        var source = new DirectoryInfo(sourcePath);
        Directory.CreateDirectory(destinationPath);

        foreach (var file in source.GetFiles())
        {
            file.CopyTo(Path.Combine(destinationPath, file.Name), overwrite: true);
        }

        foreach (var directory in source.GetDirectories())
        {
            CopyDirectory(directory.FullName, Path.Combine(destinationPath, directory.Name));
        }
    }
}
