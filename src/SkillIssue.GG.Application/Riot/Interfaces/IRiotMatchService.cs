using SkillIssue.GG.Application.Riot.Models;

namespace SkillIssue.GG.Application.Riot.Interfaces;

public interface IRiotMatchService
{
    Task<RiotMatchDetails> GetMatchAsync(
        string matchId,
        CancellationToken cancellationToken = default);
}
