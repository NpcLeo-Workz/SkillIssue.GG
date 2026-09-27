using SkillIssue.GG.Application.Matches.Interfaces;
using SkillIssue.GG.Domain.Entities;

namespace SkillIssue.GG.Application.Matches.Services;

public sealed class MatchQueryService(IMatchRepository matchRepository) : IMatchQueryService
{
    private readonly IMatchRepository _matchRepository = matchRepository;

    public Task<Match?> GetByRiotMatchIdAsync(
        string riotMatchId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(riotMatchId);

        return _matchRepository.GetByRiotMatchIdAsync(
            riotMatchId,
            cancellationToken);
    }
}
