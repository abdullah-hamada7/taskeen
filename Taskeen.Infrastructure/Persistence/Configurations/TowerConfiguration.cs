using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskeen.Domain.Entities;

namespace Taskeen.Infrastructure.Persistence.Configurations;

public class TowerConfiguration : IEntityTypeConfiguration<Tower>
{
    public void Configure(EntityTypeBuilder<Tower> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(50);
        builder.HasIndex(x => x.Name).IsUnique();

        builder.HasMany(x => x.Units)
            .WithOne(x => x.Tower)
            .HasForeignKey(x => x.TowerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
