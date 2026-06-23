using Microsoft.CodeAnalysis.CSharp;
using KarpathySkillsBenchmark.Runners;

namespace KarpathySkillsBenchmark.Metrics;

public sealed class ComplexityMetricCollector : IMetricCollector
{
    public string Name => "complexity";

    public Task<MetricSet> CollectAsync(RunContext context, AgentRunResult result, CancellationToken cancellationToken)
    {
        var csFiles = Directory.Exists(context.WorkspacePath)
            ? Directory.EnumerateFiles(context.WorkspacePath, "*.cs", SearchOption.AllDirectories).Take(50).ToList()
            : [];

        var syntaxNodes = 0;
        foreach (var file in csFiles)
        {
            var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(file));
            syntaxNodes += tree.GetRoot(cancellationToken).DescendantNodes().Count();
        }

        return Task.FromResult(new MetricSet()
            .Add("tool_calls", result.ToolCalls.Count)
            .Add("prompt_characters", context.Prompt.Length)
            .Add("workspace_cs_files", csFiles.Count)
            .Add("syntax_nodes", syntaxNodes));
    }
}
