using SkillIssue.GG.Application.Riot.Models;

namespace SkillIssue.GG.Application.Riot.Interfaces;

public interface IRiotAccountService
{
    Task<RiotAccount> GetByRiotIdAsync(
        string gameName,
        string tagLine,
        CancellationToken cancellationToken = default);
}
