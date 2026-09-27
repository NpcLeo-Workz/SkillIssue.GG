using SkillIssue.GG.Application.Matches.Interfaces;
using SkillIssue.GG.Domain.Entities;

namespace SkillIssue.GG.Application.Matches.Services;

public sealed class PlayerMatchHistoryService(IMatchRepository matchRepository) : IPlayerMatchHistoryService
{
    private readonly IMatchRepository _matchRepository = matchRepository;

    public Task<IReadOnlyList<Match>> GetAsync(
        string puuid,
        int skip = 0,
        int take = 20,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(puuid);

        if (skip < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(skip),
                skip,
                "Skip must be zero or greater.");
        }

        if (take is < 1 or > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(take),
                take,
                "Take must be between 1 and 100.");
        }

        return _matchRepository.GetByPlayerPuuidAsync(
            puuid,
            skip,
            take,
            cancellationToken);
    }
}
