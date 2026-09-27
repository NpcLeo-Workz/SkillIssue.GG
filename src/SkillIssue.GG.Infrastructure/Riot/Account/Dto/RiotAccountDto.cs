using System.Text.Json.Serialization;

namespace SkillIssue.GG.Infrastructure.Riot.Account.Dto;

public sealed class RiotAccountDto
{
    [JsonPropertyName("puuid")]
    public string Puuid { get; init; } = string.Empty;

    [JsonPropertyName("gameName")]
    public string GameName { get; init; } = string.Empty;

    [JsonPropertyName("tagLine")]
    public string TagLine { get; init; } = string.Empty;
}
