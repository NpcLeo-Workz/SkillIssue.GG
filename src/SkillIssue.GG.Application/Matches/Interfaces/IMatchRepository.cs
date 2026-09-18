using SkillIssue.GG.Domain.Entities;

namespace SkillIssue.GG.Application.Matches.Interfaces;

public interface IMatchRepository
{
    Task<bool> ExistsByRiotMatchIdAsync(
        string riotMatchId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Match match,
        CancellationToken cancellationToken = default);
}
