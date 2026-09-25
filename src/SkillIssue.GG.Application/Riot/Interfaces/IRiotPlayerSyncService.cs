using SkillIssue.GG.Application.Riot.Models;

namespace SkillIssue.GG.Application.Riot.Interfaces;

public interface IRiotPlayerSyncService
{
    Task<RiotPlayerSyncResult> SyncAsync(
        string gameName,
        string tagLine,
        string region,
        CancellationToken cancellationToken = default);
}
