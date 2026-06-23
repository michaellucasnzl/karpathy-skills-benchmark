using System.ComponentModel.DataAnnotations;

namespace KarpathySkillsBenchmark.Configuration;

public sealed class BenchmarkOptions
{
    [Required]
    public string DefaultAgent { get; set; } = "opencode";

    [Range(1, 100)]
    public int DefaultRepeats { get; set; } = 1;

    [Range(1, 240)]
    public int DefaultTimeoutMinutes { get; set; } = 15;

    [Required]
    public string ResultsDirectory { get; set; } = "results";

    [Required]
    public string FixturesDirectory { get; set; } = "fixtures";

    [Required]
    public string TasksDirectory { get; set; } = "tasks";

    [Required]
    public string SkillsFilePath { get; set; } = "skills/AGENTS.md";
}
