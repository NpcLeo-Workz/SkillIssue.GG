using System.Text.Json.Serialization;

namespace SkillIssue.GG.Infrastructure.Riot.Match.Dto;

public sealed class RiotMatchDto
{
    [JsonPropertyName("metadata")]
    public RiotMatchMetadataDto? Metadata { get; init; }

    [JsonPropertyName("info")]
    public RiotMatchInfoDto? Info { get; init; }
}
