using Microsoft.EntityFrameworkCore;
using SkillIssue.GG.Application.Players.Interfaces;
using SkillIssue.GG.Domain.Entities;

namespace SkillIssue.GG.Infrastructure.Persistence.Repositories;

public sealed class PlayerRepository(SkillIssueDbContext dbContext) : IPlayerRepository
{
    private readonly SkillIssueDbContext _dbContext = dbContext;

    public Task<Player?> GetByPuuidAsync(
        string puuid,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(puuid);

        return _dbContext.Players
            .AsNoTracking()
            .SingleOrDefaultAsync(
                player => player.Puuid == puuid,
                cancellationToken);
    }

    public async Task AddAsync(
        Player player,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(player);

        await _dbContext.Players.AddAsync(
            player,
            cancellationToken);

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }
}
