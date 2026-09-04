using Microsoft.EntityFrameworkCore;
using SkillIssue.GG.Domain.Entities;

namespace SkillIssue.GG.Infrastructure.Persistence;

public class SkillIssueDbContext(
    DbContextOptions<SkillIssueDbContext> options) : DbContext(options)
{
    public DbSet<Player> Players => Set<Player>();

    public DbSet<Match> Matches => Set<Match>();

    public DbSet<MatchParticipant> MatchParticipants => Set<MatchParticipant>();

    public DbSet<Champion> Champions => Set<Champion>();

    public DbSet<Item> Items => Set<Item>();

    public DbSet<Rune> Runes => Set<Rune>();

    public DbSet<Patch> Patches => Set<Patch>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(SkillIssueDbContext).Assembly);
    }
}
