using System.Diagnostics;
using KarpathySkillsBenchmark.Runners;

namespace KarpathySkillsBenchmark.Verification;

public sealed class TestRunnerVerifier : IVerifier
{
    public string Name => "test-runner";

    public async Task<VerificationResult> VerifyAsync(RunContext context, AgentRunResult result, CancellationToken cancellationToken)
    {
        var steps = new List<VerificationStepResult>();
        foreach (var criterion in context.TaskDefinition.SuccessCriteria.Where(x => string.Equals(x.Type, "test", StringComparison.OrdinalIgnoreCase)))
        {
            var execution = await RunCommandAsync(criterion.Command!, context.WorkspacePath, cancellationToken);
            var passed = criterion.ExpectPass ? execution.ExitCode == 0 : execution.ExitCode != 0;
            steps.Add(new VerificationStepResult
            {
                Name = criterion.Command!,
                Passed = passed,
                Summary = execution.Output
            });
        }

        return new VerificationResult
        {
            Passed = steps.All(step => step.Passed),
            Score = steps.Count == 0 ? 0 : steps.Count(step => step.Passed) / (double)steps.Count,
            Summary = steps.Count == 0 ? "No test criteria defined." : $"Executed {steps.Count} test verification step(s).",
            Steps = steps
        };
    }

    private static async Task<(int ExitCode, string Output)> RunCommandAsync(string command, string workingDirectory, CancellationToken cancellationToken)
    {
        var startInfo = OperatingSystem.IsWindows()
            ? new ProcessStartInfo("cmd.exe", $"/c {command}")
            : new ProcessStartInfo("/bin/bash", $"-lc \"{command.Replace("\"", "\\\"")}\"");
        startInfo.WorkingDirectory = workingDirectory;
        startInfo.UseShellExecute = false;
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;

        using var process = new Process { StartInfo = startInfo };
        process.Start();
        var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);
        return (process.ExitCode, (await stdoutTask) + (await stderrTask));
    }
}
