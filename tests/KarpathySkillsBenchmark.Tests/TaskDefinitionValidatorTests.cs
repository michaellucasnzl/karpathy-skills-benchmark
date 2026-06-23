using KarpathySkillsBenchmark.Tasks;

namespace KarpathySkillsBenchmark.Tests;

public sealed class TaskDefinitionValidatorTests
{
    [Fact]
    public void InvalidDefinition_FailsValidation()
    {
        var validator = new TaskDefinitionValidator();
        var result = validator.Validate(new TaskDefinition
        {
            Id = string.Empty,
            Title = string.Empty,
            Difficulty = string.Empty,
            Fixture = string.Empty,
            Prompt = string.Empty,
            SuccessCriteria = [new SuccessCriterion { Type = "llmJudge" }]
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Id");
        Assert.Contains(result.Errors, error => error.PropertyName.Contains("SuccessCriteria"));
    }
}
