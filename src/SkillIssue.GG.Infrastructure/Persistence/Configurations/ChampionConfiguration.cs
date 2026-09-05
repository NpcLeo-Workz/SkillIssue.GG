using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillIssue.GG.Domain.Entities;

namespace SkillIssue.GG.Infrastructure.Persistence.Configurations;

public class ChampionConfiguration : IEntityTypeConfiguration<Champion>
{
    public void Configure(EntityTypeBuilder<Champion> builder)
    {
        builder.ToTable("champions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.RiotChampionId)
            .IsRequired();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(x => x.RiotChampionId)
            .IsUnique();

        builder.HasIndex(x => x.Name);
    }
}
