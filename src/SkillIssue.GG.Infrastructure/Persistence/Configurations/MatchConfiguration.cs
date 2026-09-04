using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillIssue.GG.Domain.Entities;

namespace SkillIssue.GG.Infrastructure.Persistence.Configurations;

public class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> builder)
    {
        builder.ToTable("matches");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.RiotMatchId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.RiotGameId)
            .IsRequired();

        builder.Property(x => x.DataVersion)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.GameVersion)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.GameMode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.GameType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.PlatformId)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.GameCreatedAt)
            .IsRequired();

        builder.Property(x => x.StartedAt)
            .IsRequired();

        builder.Property(x => x.EndedAt)
            .IsRequired(false);

        builder.Property(x => x.Duration)
            .IsRequired();

        builder.Property(x => x.EndOfGameResult)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.HasIndex(x => x.RiotMatchId)
            .IsUnique();

        builder.HasIndex(x => x.RiotGameId);

        builder.HasIndex(x => x.StartedAt);
    }
}
