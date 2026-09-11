using SkillIssue.GG.Application.Riot.Models;

namespace SkillIssue.GG.Application.Riot.Interfaces;

public interface IRiotMatchHistorySyncService
{
    Task<RiotMatchHistorySyncResult> SyncAsync(
        string puuid,
        int start = 0,
        int count = 20,
        CancellationToken cancellationToken = default);
}
