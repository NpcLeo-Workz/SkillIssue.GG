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
    Task<IReadOnlyList<Match>> GetByPlayerPuuidAsync(
        string puuid,
        int skip,
        int take,
        CancellationToken cancellationToken = default);
}
