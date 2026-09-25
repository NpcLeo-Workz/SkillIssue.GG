using SkillIssue.GG.Application.Matches.Interfaces;
using SkillIssue.GG.Application.Riot.Interfaces;
using SkillIssue.GG.Application.Riot.Mapping;
using SkillIssue.GG.Application.Riot.Models;

namespace SkillIssue.GG.Application.Riot.Services;

public sealed class RiotMatchImportService(IRiotMatchService riotMatchService,
        IMatchRepository matchRepository) : IRiotMatchImportService
{
    private readonly IRiotMatchService _riotMatchService = riotMatchService;
    private readonly IMatchRepository _matchRepository = matchRepository;


    public async Task<RiotMatchImportResult> ImportAsync(
        string matchId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(matchId);

        var exists = await _matchRepository.ExistsByRiotMatchIdAsync(
            matchId,
            cancellationToken);

        if (exists)
        {
            return new RiotMatchImportResult(
                matchId,
                Imported: false,
                MatchId: null);
        }

        var riotMatch = await _riotMatchService.GetMatchAsync(
            matchId,
            cancellationToken);

        var match = RiotMatchDomainMapper.Map(riotMatch);

        await _matchRepository.AddAsync(
            match,
            cancellationToken);

        return new RiotMatchImportResult(
            match.RiotMatchId,
            Imported: true,
            MatchId: match.Id);
    }
}
