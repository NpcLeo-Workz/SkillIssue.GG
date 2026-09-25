namespace SkillIssue.GG.Application.Riot.Models;

public sealed record RiotPlayerSyncResult(
    Guid PlayerId,
    string Puuid,
    bool Created);
