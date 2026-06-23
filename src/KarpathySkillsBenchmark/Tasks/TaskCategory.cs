using System.Text.Json.Serialization;

namespace KarpathySkillsBenchmark.Tasks;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TaskCategory
{
    [JsonStringEnumMemberName("bug-fixes")]
    BugFixes,

    [JsonStringEnumMemberName("features")]
    Features,

    [JsonStringEnumMemberName("refactors")]
    Refactors,

    [JsonStringEnumMemberName("greenfield")]
    Greenfield,

    [JsonStringEnumMemberName("trivial")]
    Trivial,

    [JsonStringEnumMemberName("clarifying")]
    Clarifying
}
