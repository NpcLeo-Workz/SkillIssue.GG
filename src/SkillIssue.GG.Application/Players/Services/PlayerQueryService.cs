using SkillIssue.GG.Application.Players.Interfaces;
using SkillIssue.GG.Domain.Entities;

namespace SkillIssue.GG.Application.Players.Services;

public sealed class PlayerQueryService(IPlayerRepository playerRepository) : IPlayerQueryService
{
    private readonly IPlayerRepository _playerRepository = playerRepository;

    public Task<Player?> GetByPuuidAsync(
        string puuid,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(puuid);

        return _playerRepository.GetByPuuidAsync(
            puuid,
            cancellationToken);
    }
}
