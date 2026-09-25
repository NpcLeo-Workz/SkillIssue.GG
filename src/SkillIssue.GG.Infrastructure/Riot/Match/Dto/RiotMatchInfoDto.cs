using System.Text.Json.Serialization;

namespace SkillIssue.GG.Infrastructure.Riot.Match.Dto;

public sealed class RiotMatchInfoDto
{
    [JsonPropertyName("gameId")]
    public long GameId { get; init; }

    [JsonPropertyName("gameVersion")]
    public string GameVersion { get; init; } = string.Empty;

    [JsonPropertyName("gameMode")]
    public string GameMode { get; init; } = string.Empty;

    [JsonPropertyName("gameType")]
    public string GameType { get; init; } = string.Empty;

    [JsonPropertyName("mapId")]
    public int MapId { get; init; }

    [JsonPropertyName("queueId")]
    public int QueueId { get; init; }

    [JsonPropertyName("platformId")]
    public string PlatformId { get; init; } = string.Empty;

    [JsonPropertyName("gameCreation")]
    public long GameCreation { get; init; }

    [JsonPropertyName("gameStartTimestamp")]
    public long GameStartTimestamp { get; init; }

    [JsonPropertyName("gameEndTimestamp")]
    public long? GameEndTimestamp { get; init; }

    [JsonPropertyName("gameDuration")]
    public long GameDuration { get; init; }

    [JsonPropertyName("endOfGameResult")]
    public string? EndOfGameResult { get; init; }

    [JsonPropertyName("participants")]
    public IReadOnlyList<RiotMatchParticipantDto>? Participants { get; init; }
}
