using SkillIssue.GG.Domain.Entities;

namespace SkillIssue.GG.Application.Players.Interfaces;

public interface IPlayerRepository
{
    Task<Player?> GetByPuuidAsync(
        string puuid,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Player player,
        CancellationToken cancellationToken = default);
}
