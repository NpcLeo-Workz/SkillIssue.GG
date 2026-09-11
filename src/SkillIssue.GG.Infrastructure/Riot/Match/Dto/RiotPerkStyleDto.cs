using System.Text.Json.Serialization;

namespace SkillIssue.GG.Infrastructure.Riot.Match.Dto;

public sealed class RiotPerkStyleDto
{
    [JsonPropertyName("style")]
    public int Style { get; init; }

    [JsonPropertyName("selections")]
    public IReadOnlyList<RiotPerkSelectionDto>? Selections { get; init; }
}
