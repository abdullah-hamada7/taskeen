using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskeen.Domain.Entities;

namespace Taskeen.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FullName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.IdentityNumber).IsRequired().HasMaxLength(20);
        builder.HasIndex(x => x.IdentityNumber).IsUnique();
        builder.Property(x => x.Role).HasConversion<string>().IsRequired();

        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasMany(x => x.OwnedUnits)
            .WithOne(x => x.Owner)
            .HasForeignKey(x => x.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.SupervisedUnits)
            .WithOne(x => x.Supervisor)
            .HasForeignKey(x => x.SupervisorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
