namespace SkillIssue.GG.Web.Models.Players;

public sealed record PlayerResponse(
    Guid Id,
    string Puuid,
    string Name,
    string Region);
