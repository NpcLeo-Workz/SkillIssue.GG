using Microsoft.EntityFrameworkCore;
using SkillIssue.GG.Application.Matches.Interfaces;
using SkillIssue.GG.Domain.Entities;

namespace SkillIssue.GG.Infrastructure.Persistence.Repositories;

public sealed class MatchRepository(SkillIssueDbContext dbContext) : IMatchRepository
{
    private readonly SkillIssueDbContext _dbContext = dbContext;

    public Task<bool> ExistsByRiotMatchIdAsync(
        string riotMatchId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(riotMatchId);

        return _dbContext.Matches
            .AsNoTracking()
            .AnyAsync(
                match => match.RiotMatchId == riotMatchId,
                cancellationToken);
    }

    public async Task AddAsync(
        Match match,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(match);

        await _dbContext.Matches.AddAsync(
            match,
            cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
