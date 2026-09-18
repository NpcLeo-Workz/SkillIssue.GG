using SkillIssue.GG.Application.Riot.Interfaces;
using SkillIssue.GG.Application.Riot.Models;

namespace SkillIssue.GG.Application.Riot.Services;

public sealed class RiotPlayerAndMatchSyncService
    : IRiotPlayerAndMatchSyncService
{
    private readonly IRiotPlayerSyncService _playerSyncService;
    private readonly IRiotMatchHistorySyncService _matchHistorySyncService;

    public RiotPlayerAndMatchSyncService(
        IRiotPlayerSyncService playerSyncService,
        IRiotMatchHistorySyncService matchHistorySyncService)
    {
        _playerSyncService = playerSyncService;
        _matchHistorySyncService = matchHistorySyncService;
    }

    public async Task<RiotPlayerAndMatchSyncResult> SyncAsync(
        string gameName,
        string tagLine,
        string region,
        int start = 0,
        int count = 20,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(gameName);
        ArgumentException.ThrowIfNullOrWhiteSpace(tagLine);
        ArgumentException.ThrowIfNullOrWhiteSpace(region);

        if (start < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(start),
                start,
                "Start must be zero or greater.");
        }

        if (count is < 1 or > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(count),
                count,
                "Count must be between 1 and 100.");
        }

        var playerResult = await _playerSyncService.SyncAsync(
            gameName,
            tagLine,
            region,
            cancellationToken);

        var matchResult = await _matchHistorySyncService.SyncAsync(
            playerResult.Puuid,
            start,
            count,
            cancellationToken);

        return new RiotPlayerAndMatchSyncResult(
            PlayerId: playerResult.PlayerId,
            Puuid: playerResult.Puuid,
            PlayerCreated: playerResult.Created,
            RequestedMatches: matchResult.Requested,
            ImportedMatches: matchResult.Imported,
            SkippedMatches: matchResult.Skipped);
    }
}
