using System.Text.Json.Serialization;

namespace SkillIssue.GG.Infrastructure.Riot.Match.Dto;

public sealed class RiotMatchParticipantDto
{
    [JsonPropertyName("puuid")]
    public string Puuid { get; init; } = string.Empty;

    [JsonPropertyName("participantId")]
    public int ParticipantId { get; init; }

    [JsonPropertyName("teamId")]
    public int TeamId { get; init; }

    [JsonPropertyName("championId")]
    public int ChampionId { get; init; }

    [JsonPropertyName("championName")]
    public string ChampionName { get; init; } = string.Empty;

    [JsonPropertyName("teamPosition")]
    public string TeamPosition { get; init; } = string.Empty;

    [JsonPropertyName("kills")]
    public int Kills { get; init; }

    [JsonPropertyName("deaths")]
    public int Deaths { get; init; }

    [JsonPropertyName("assists")]
    public int Assists { get; init; }

    [JsonPropertyName("goldEarned")]
    public int GoldEarned { get; init; }

    [JsonPropertyName("goldSpent")]
    public int GoldSpent { get; init; }

    [JsonPropertyName("totalMinionsKilled")]
    public int TotalMinionsKilled { get; init; }

    [JsonPropertyName("neutralMinionsKilled")]
    public int NeutralMinionsKilled { get; init; }

    [JsonPropertyName("visionScore")]
    public int VisionScore { get; init; }

    [JsonPropertyName("wardsPlaced")]
    public int WardsPlaced { get; init; }

    [JsonPropertyName("wardsKilled")]
    public int WardsKilled { get; init; }

    [JsonPropertyName("totalDamageDealt")]
    public int TotalDamageDealt { get; init; }

    [JsonPropertyName("totalDamageDealtToChampions")]
    public int TotalDamageDealtToChampions { get; init; }

    [JsonPropertyName("totalDamageTaken")]
    public int TotalDamageTaken { get; init; }

    [JsonPropertyName("timePlayed")]
    public int TimePlayed { get; init; }

    [JsonPropertyName("win")]
    public bool Win { get; init; }

    [JsonPropertyName("item0")]
    public int Item0 { get; init; }

    [JsonPropertyName("item1")]
    public int Item1 { get; init; }

    [JsonPropertyName("item2")]
    public int Item2 { get; init; }

    [JsonPropertyName("item3")]
    public int Item3 { get; init; }

    [JsonPropertyName("item4")]
    public int Item4 { get; init; }

    [JsonPropertyName("item5")]
    public int Item5 { get; init; }

    [JsonPropertyName("item6")]
    public int Item6 { get; init; }

    [JsonPropertyName("perks")]
    public RiotPerksDto? Perks { get; init; }
}
