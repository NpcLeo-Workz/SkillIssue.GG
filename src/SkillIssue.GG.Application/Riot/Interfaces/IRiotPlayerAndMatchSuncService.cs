using SkillIssue.GG.Application.Riot.Models;

namespace SkillIssue.GG.Application.Riot.Interfaces;

public interface IRiotPlayerAndMatchSyncService
{
    Task<RiotPlayerAndMatchSyncResult> SyncAsync(
        string gameName,
        string tagLine,
        string region,
        int start = 0,
        int count = 20,
        CancellationToken cancellationToken = default);
}
