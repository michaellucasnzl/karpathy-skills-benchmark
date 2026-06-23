using FluentValidation;

namespace KarpathySkillsBenchmark.Tasks;

public sealed class TaskDefinition
{
    public string Id { get; set; } = string.Empty;

    public TaskCategory Category { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Difficulty { get; set; } = string.Empty;

    public string Fixture { get; set; } = string.Empty;

    public string StartingCommit { get; set; } = "main";

    public string Prompt { get; set; } = string.Empty;

    public List<string> ExpectedBehavior { get; set; } = [];

    public List<SuccessCriterion> SuccessCriteria { get; set; } = [];

    public List<string> Temptations { get; set; } = [];

    public List<string> Tags { get; set; } = [];

    public int EstimatedLinesChanged { get; set; }

    public int TimeoutMinutes { get; set; } = 15;
}

public sealed class SuccessCriterion
{
    public string Type { get; set; } = string.Empty;

    public string? Command { get; set; }

    public bool ExpectPass { get; set; } = true;

    public string? Rubric { get; set; }

    public double? MinScore { get; set; }
}

public sealed class TaskDefinitionValidator : AbstractValidator<TaskDefinition>
{
    public TaskDefinitionValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Title).NotEmpty();
        RuleFor(x => x.Difficulty).NotEmpty();
        RuleFor(x => x.Fixture).NotEmpty();
        RuleFor(x => x.Prompt).NotEmpty();
        RuleFor(x => x.TimeoutMinutes).GreaterThan(0);
        RuleFor(x => x.EstimatedLinesChanged).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SuccessCriteria).NotEmpty();
        RuleForEach(x => x.SuccessCriteria).SetValidator(new SuccessCriterionValidator());
    }
}

public sealed class SuccessCriterionValidator : AbstractValidator<SuccessCriterion>
{
    public SuccessCriterionValidator()
    {
        RuleFor(x => x.Type).NotEmpty();

        When(x => string.Equals(x.Type, "test", StringComparison.OrdinalIgnoreCase), () =>
        {
            RuleFor(x => x.Command).NotEmpty();
        });

        When(x => string.Equals(x.Type, "llmJudge", StringComparison.OrdinalIgnoreCase), () =>
        {
            RuleFor(x => x.Rubric).NotEmpty();
            RuleFor(x => x.MinScore).NotNull().InclusiveBetween(1, 5);
        });
    }
}
