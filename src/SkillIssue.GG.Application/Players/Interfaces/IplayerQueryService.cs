using SkillIssue.GG.Domain.Entities;

namespace SkillIssue.GG.Application.Players.Interfaces;

public interface IPlayerQueryService
{
    Task<Player?> GetByPuuidAsync(
        string puuid,
        CancellationToken cancellationToken = default);
}
