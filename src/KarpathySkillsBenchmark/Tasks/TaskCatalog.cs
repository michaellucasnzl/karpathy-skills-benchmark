using FluentValidation;
using FluentValidation.Results;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace KarpathySkillsBenchmark.Tasks;

public sealed class TaskCatalog
{
    private readonly IValidator<TaskDefinition> _validator;

    public TaskCatalog(IValidator<TaskDefinition> validator)
    {
        _validator = validator;
    }

    public static JsonSerializerOptions SerializerOptions { get; } = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public IReadOnlyList<TaskDefinition> Load(string tasksDirectory)
    {
        if (!Directory.Exists(tasksDirectory))
        {
            return [];
        }

        var tasks = new List<TaskDefinition>();
        foreach (var file in Directory.EnumerateFiles(tasksDirectory, "*.json", SearchOption.AllDirectories).OrderBy(path => path))
        {
            var content = File.ReadAllText(file);
            var definition = JsonSerializer.Deserialize<TaskDefinition>(content, SerializerOptions)
                ?? throw new InvalidOperationException($"Could not deserialize task file '{file}'.");
            tasks.Add(definition);
        }

        return tasks;
    }

    public IReadOnlyList<TaskDefinition> Filter(IEnumerable<TaskDefinition> tasks, string? pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
        {
            return tasks.ToList();
        }

        var regexPattern = "^" + Regex.Escape(pattern)
            .Replace("\\*", ".*")
            .Replace("\\?", ".") + "$";
        var regex = new Regex(regexPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);

        return tasks.Where(task =>
                regex.IsMatch(task.Id) ||
                regex.IsMatch(task.Title) ||
                regex.IsMatch(task.Category.ToString()) ||
                regex.IsMatch(task.Fixture))
            .ToList();
    }

    public IReadOnlyList<ValidationFailure> Validate(IEnumerable<TaskDefinition> tasks)
    {
        var failures = new List<ValidationFailure>();
        foreach (var task in tasks)
        {
            failures.AddRange(_validator.Validate(task).Errors);
        }

        return failures;
    }
}
