namespace SkillIssue.GG.Web.Models.Matches;

public sealed record PlayerMatchResponse(
    string RiotMatchId,
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
    IReadOnlyList<PlayerMatchParticipantResponse> Participants);
