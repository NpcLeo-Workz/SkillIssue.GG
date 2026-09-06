namespace SkillIssue.GG.Application.Riot.Interfaces;

public interface IRiotMatchHistoryService
{
    Task<IReadOnlyList<string>> GetMatchIdsAsync(
        string puuid,
        int start = 0,
        int count = 20,
        CancellationToken cancellationToken = default);
}
