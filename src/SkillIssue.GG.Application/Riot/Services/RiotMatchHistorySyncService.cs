using SkillIssue.GG.Application.Riot.Interfaces;
using SkillIssue.GG.Application.Riot.Models;

namespace SkillIssue.GG.Application.Riot.Services;

public sealed class RiotMatchHistorySyncService(
    IRiotMatchHistoryService matchHistoryService,
    IRiotMatchImportService matchImportService)
        : IRiotMatchHistorySyncService
{
    private readonly IRiotMatchHistoryService _matchHistoryService = matchHistoryService;
    private readonly IRiotMatchImportService _matchImportService = matchImportService;

    public async Task<RiotMatchHistorySyncResult> SyncAsync(
        string puuid,
        int start = 0,
        int count = 20,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(puuid);

        if (start < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(start),
                "Start must be greater than or equal to 0.");
        }

        if (count is < 1 or > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(count),
                "Count must be between 1 and 100.");
        }

        var matchIds = await _matchHistoryService.GetMatchIdsAsync(
            puuid,
            start,
            count,
            cancellationToken);

        var imported = 0;
        var skipped = 0;

        foreach (var matchId in matchIds)
        {
            var result = await _matchImportService.ImportAsync(
                matchId,
                cancellationToken);

            if (result.Imported)
            {
                imported++;
            }
            else
            {
                skipped++;
            }
        }

        return new RiotMatchHistorySyncResult(
            Requested: matchIds.Count,
            Imported: imported,
            Skipped: skipped);
    }
}
