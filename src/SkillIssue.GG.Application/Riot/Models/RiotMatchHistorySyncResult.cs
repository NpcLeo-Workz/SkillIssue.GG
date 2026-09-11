namespace SkillIssue.GG.Application.Riot.Models;

public sealed record RiotMatchHistorySyncResult(
    int Requested,
    int Imported,
    int Skipped);
