namespace SkillIssue.GG.Application.Riot.Models;

public sealed record RiotMatchDetails(
    string DataVersion,
    string RiotMatchId,
    long RiotGameId,
    string GameVersion,
    string GameMode,
    string GameType,
    int MapId,
    int QueueId,
    string PlatformId,
    DateTimeOffset GameCreatedAt,
    DateTimeOffset StartedAt,
    DateTimeOffset? EndedAt,
    TimeSpan Duration,
    string? EndOfGameResult,
    IReadOnlyList<RiotMatchParticipant> Participants);
