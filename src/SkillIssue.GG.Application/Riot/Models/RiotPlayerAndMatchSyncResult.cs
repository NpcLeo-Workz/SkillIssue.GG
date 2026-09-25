namespace SkillIssue.GG.Application.Riot.Models;

public sealed record RiotPlayerAndMatchSyncResult(
    Guid PlayerId,
    string Puuid,
    bool PlayerCreated,
    int RequestedMatches,
    int ImportedMatches,
    int SkippedMatches);
