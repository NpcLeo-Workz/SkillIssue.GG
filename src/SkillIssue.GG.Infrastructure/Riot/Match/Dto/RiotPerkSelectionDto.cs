using System.Text.Json.Serialization;

namespace SkillIssue.GG.Infrastructure.Riot.Match.Dto;

public sealed class RiotPerkSelectionDto
{
    [JsonPropertyName("perk")]
    public int Perk { get; init; }
}
