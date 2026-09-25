using System.Text.Json.Serialization;

namespace SkillIssue.GG.Infrastructure.Riot.Match.Dto;

public sealed class RiotPerksDto
{
    [JsonPropertyName("styles")]
    public IReadOnlyList<RiotPerkStyleDto>? Styles { get; init; }
}
