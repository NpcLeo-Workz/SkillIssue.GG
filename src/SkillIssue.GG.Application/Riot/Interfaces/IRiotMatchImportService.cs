using SkillIssue.GG.Application.Riot.Models;

namespace SkillIssue.GG.Application.Riot.Interfaces;

public interface IRiotMatchImportService
{
    Task<RiotMatchImportResult> ImportAsync(
        string matchId,
        CancellationToken cancellationToken = default);
}
