namespace SkillIssue.GG.Web.Models.Api;

public sealed record RiotSyncResponse(
    Guid PlayerId,
    string Puuid,
    bool PlayerCreated,
    int RequestedMatches,
    int ImportedMatches,
    int SkippedMatches);
