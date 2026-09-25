using SkillIssue.GG.Application.Players.Interfaces;
using SkillIssue.GG.Application.Riot.Interfaces;
using SkillIssue.GG.Application.Riot.Models;
using SkillIssue.GG.Domain.Entities;

namespace SkillIssue.GG.Application.Riot.Services;

public sealed class RiotPlayerSyncService : IRiotPlayerSyncService
{
    private readonly IRiotAccountService _riotAccountService;
    private readonly IPlayerRepository _playerRepository;

    public RiotPlayerSyncService(
        IRiotAccountService riotAccountService,
        IPlayerRepository playerRepository)
    {
        _riotAccountService = riotAccountService;
        _playerRepository = playerRepository;
    }

    public async Task<RiotPlayerSyncResult> SyncAsync(
        string gameName,
        string tagLine,
        string region,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(gameName);
        ArgumentException.ThrowIfNullOrWhiteSpace(tagLine);
        ArgumentException.ThrowIfNullOrWhiteSpace(region);

        var account = await _riotAccountService.GetByRiotIdAsync(
            gameName,
            tagLine,
            cancellationToken);

        var existingPlayer = await _playerRepository.GetByPuuidAsync(
            account.Puuid,
            cancellationToken);

        if (existingPlayer is not null)
        {
            return new RiotPlayerSyncResult(
                existingPlayer.Id,
                existingPlayer.Puuid,
                Created: false);
        }

        var player = new Player(
            account.Puuid,
            account.GameName,
            region);

        await _playerRepository.AddAsync(
            player,
            cancellationToken);

        return new RiotPlayerSyncResult(
            player.Id,
            player.Puuid,
            Created: true);
    }
}
