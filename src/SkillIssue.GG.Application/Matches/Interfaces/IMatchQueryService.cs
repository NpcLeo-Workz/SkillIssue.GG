using SkillIssue.GG.Domain.Entities;

namespace SkillIssue.GG.Application.Matches.Interfaces;

public interface IMatchQueryService
{
    Task<Match?> GetByRiotMatchIdAsync(
        string riotMatchId,
        CancellationToken cancellationToken = default);
}
