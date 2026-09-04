using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillIssue.GG.Domain.Entities;

namespace SkillIssue.GG.Infrastructure.Persistence.Configurations;

public class MatchParticipantConfiguration
    : IEntityTypeConfiguration<MatchParticipant>
{
    public void Configure(EntityTypeBuilder<MatchParticipant> builder)
    {
        builder.ToTable("match_participants");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.MatchId)
            .IsRequired();

        builder.Property(x => x.PlayerPuuid)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.ParticipantId)
            .IsRequired();

        builder.Property(x => x.TeamId)
            .IsRequired();

        builder.Property(x => x.ChampionId)
            .IsRequired();

        builder.Property(x => x.TeamPosition)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.Kills).IsRequired();
        builder.Property(x => x.Deaths).IsRequired();
        builder.Property(x => x.Assists).IsRequired();

        builder.Property(x => x.GoldEarned).IsRequired();
        builder.Property(x => x.GoldSpent).IsRequired();

        builder.Property(x => x.TotalMinionsKilled).IsRequired();
        builder.Property(x => x.NeutralMinionsKilled).IsRequired();

        builder.Property(x => x.VisionScore).IsRequired();
        builder.Property(x => x.WardsPlaced).IsRequired();
        builder.Property(x => x.WardsKilled).IsRequired();

        builder.Property(x => x.TotalDamageDealtToChampions).IsRequired();
        builder.Property(x => x.TotalDamageTaken).IsRequired();

        builder.Property(x => x.TimePlayed)
            .IsRequired();

        builder.Property(x => x.Won)
            .IsRequired();

        builder.HasIndex(x => new { x.MatchId, x.ParticipantId })
            .IsUnique();

        builder.HasIndex(x => x.PlayerPuuid);

        builder.HasIndex(x => x.ChampionId);

        builder.PrimitiveCollection(x => x.ItemIds)
            .HasColumnName("item_ids")
            .HasColumnType("integer[]");

        builder.PrimitiveCollection(x => x.RuneIds)
            .HasColumnName("rune_ids")
            .HasColumnType("integer[]");
    }
}
