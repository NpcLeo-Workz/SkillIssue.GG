namespace SkillIssue.GG.Application.Riot.Models;

public sealed record RiotMatchImportResult(
    string RiotMatchId,
    bool Imported,
    Guid? MatchId);
