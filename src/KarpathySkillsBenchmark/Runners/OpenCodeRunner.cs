using System.Diagnostics;
using System.Text;
using System.Text.Json;
using KarpathySkillsBenchmark.Fixtures;

namespace KarpathySkillsBenchmark.Runners;

public sealed class OpenCodeRunner : IAgentRunner
{
    private readonly FixtureManager _fixtureManager;

    public OpenCodeRunner(FixtureManager fixtureManager)
    {
        _fixtureManager = fixtureManager;
    }

    public async Task<AgentRunResult> RunAsync(RunContext context, CancellationToken cancellationToken)
    {
        _fixtureManager.CopyOpenCodeConfig(context.RepoRoot, context.WorkspacePath);

        var startedAt = DateTimeOffset.UtcNow;
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        linkedCts.CancelAfter(context.Timeout);

        var model = context.Model.Contains('/') ? context.Model : $"{context.AgentProfile.Provider}/{context.Model}";
        var startInfo = new ProcessStartInfo
        {
            FileName = context.AgentProfile.Executable,
            WorkingDirectory = context.WorkspacePath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        startInfo.ArgumentList.Add("run");
        startInfo.ArgumentList.Add("--dir");
        startInfo.ArgumentList.Add(context.WorkspacePath);
        startInfo.ArgumentList.Add("--model");
        startInfo.ArgumentList.Add(model);
        startInfo.ArgumentList.Add("--format");
        startInfo.ArgumentList.Add("json");
        startInfo.ArgumentList.Add("--dangerously-skip-permissions");
        startInfo.ArgumentList.Add(context.Prompt);

        using var process = new Process { StartInfo = startInfo };
        if (!process.Start())
        {
            throw new InvalidOperationException("Failed to start the OpenCode process.");
        }

        var stdoutTask = process.StandardOutput.ReadToEndAsync(linkedCts.Token);
        var stderrTask = process.StandardError.ReadToEndAsync(linkedCts.Token);
        await process.WaitForExitAsync(linkedCts.Token);

        var stdout = await stdoutTask;
        var stderr = await stderrTask;
        var result = ParseOutput(stdout);
        result.StartedAtUtc = startedAt;
        result.FinishedAtUtc = DateTimeOffset.UtcNow;
        result.ErrorOutput = stderr;
        result.ExitCode = process.ExitCode;
        result.Succeeded = process.ExitCode == 0;

        return result;
    }

    private static AgentRunResult ParseOutput(string stdout)
    {
        var tokenUsage = new TokenUsage();
        var toolCalls = new List<ToolCallRecord>();
        var rawText = new StringBuilder();

        foreach (var line in stdout.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries))
        {
            rawText.AppendLine(line);
            try
            {
                using var document = JsonDocument.Parse(line);
                var root = document.RootElement;
                var type = root.TryGetProperty("type", out var typeProperty)
                    ? typeProperty.GetString() ?? string.Empty
                    : string.Empty;

                if (type.Contains("tool", StringComparison.OrdinalIgnoreCase))
                {
                    toolCalls.Add(new ToolCallRecord
                    {
                        Name = TryReadString(root, "name") ?? type,
                        TimestampUtc = DateTimeOffset.UtcNow,
                        Details = line
                    });
                }

                tokenUsage.InputTokens = Math.Max(tokenUsage.InputTokens, FindInt(root, "input_tokens") ?? FindInt(root, "prompt_tokens") ?? 0);
                tokenUsage.OutputTokens = Math.Max(tokenUsage.OutputTokens, FindInt(root, "output_tokens") ?? FindInt(root, "completion_tokens") ?? 0);
            }
            catch (JsonException)
            {
            }
        }

        return new AgentRunResult
        {
            RawOutput = rawText.ToString(),
            TokenUsage = tokenUsage,
            ToolCalls = toolCalls
        };
    }

    private static string? TryReadString(JsonElement element, string propertyName)
    {
        if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty(propertyName, out var property))
        {
            return property.GetString();
        }

        foreach (var child in EnumerateChildren(element))
        {
            var value = TryReadString(child, propertyName);
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }

    private static int? FindInt(JsonElement element, string propertyName)
    {
        if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty(propertyName, out var property))
        {
            if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var number))
            {
                return number;
            }
        }

        foreach (var child in EnumerateChildren(element))
        {
            var value = FindInt(child, propertyName);
            if (value.HasValue)
            {
                return value;
            }
        }

        return null;
    }

    private static IEnumerable<JsonElement> EnumerateChildren(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                yield return property.Value;
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var child in element.EnumerateArray())
            {
                yield return child;
            }
        }
    }
}
