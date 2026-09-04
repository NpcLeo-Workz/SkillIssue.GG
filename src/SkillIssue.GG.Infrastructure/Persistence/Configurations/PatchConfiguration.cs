using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillIssue.GG.Domain.Entities;

namespace SkillIssue.GG.Infrastructure.Persistence.Configurations;

public class PatchConfiguration : IEntityTypeConfiguration<Patch>
{
    public void Configure(EntityTypeBuilder<Patch> builder)
    {
        builder.ToTable("patches");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Version)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.DataDragonVersion)
            .IsRequired()
            .HasMaxLength(30);

        builder.HasIndex(x => x.Version)
            .IsUnique();

        builder.HasIndex(x => x.DataDragonVersion)
            .IsUnique();
    }
}
