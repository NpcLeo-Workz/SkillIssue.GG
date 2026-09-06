using System.Text.Json.Serialization;

namespace SkillIssue.GG.Infrastructure.Riot.Match.Dto;

public sealed class RiotMatchMetadataDto
{
    [JsonPropertyName("dataVersion")]
    public string DataVersion { get; init; } = string.Empty;

    [JsonPropertyName("matchId")]
    public string MatchId { get; init; } = string.Empty;
}
