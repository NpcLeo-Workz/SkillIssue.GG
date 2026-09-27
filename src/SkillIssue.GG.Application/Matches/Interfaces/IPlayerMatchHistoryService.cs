using SkillIssue.GG.Domain.Entities;

namespace SkillIssue.GG.Application.Matches.Interfaces;

public interface IPlayerMatchHistoryService
{
    Task<IReadOnlyList<Match>> GetAsync(
        string puuid,
        int skip = 0,
        int take = 20,
        CancellationToken cancellationToken = default);
}
