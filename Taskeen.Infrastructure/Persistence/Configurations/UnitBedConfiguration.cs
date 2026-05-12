using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskeen.Domain.Entities;

namespace Taskeen.Infrastructure.Persistence.Configurations;

public class UnitBedConfiguration : IEntityTypeConfiguration<UnitBed>
{
    public void Configure(EntityTypeBuilder<UnitBed> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.UnitId, x.BedNumber }).IsUnique();

        builder.HasQueryFilter(x => !x.Unit.Owner.IsDeleted && !x.Unit.Supervisor.IsDeleted);
    }
}
