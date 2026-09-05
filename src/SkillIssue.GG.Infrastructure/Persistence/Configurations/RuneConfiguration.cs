using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillIssue.GG.Domain.Entities;

namespace SkillIssue.GG.Infrastructure.Persistence.Configurations;

public class RuneConfiguration : IEntityTypeConfiguration<Rune>
{
    public void Configure(EntityTypeBuilder<Rune> builder)
    {
        builder.ToTable("runes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.RiotRuneId)
            .IsRequired();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .IsRequired();

        builder.Property(x => x.IconPath)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.RuneTreeId)
            .IsRequired();

        builder.Property(x => x.RuneTreeName)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(x => x.RiotRuneId)
            .IsUnique();

        builder.HasIndex(x => x.Name);

        builder.HasIndex(x => x.RuneTreeId);
    }
}
