using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskeen.Domain.Entities;

namespace Taskeen.Infrastructure.Persistence.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.BaseAmount).HasPrecision(18, 2);
        builder.Property(x => x.TaxAmount).HasPrecision(18, 2);
        builder.Property(x => x.TotalAmount).HasPrecision(18, 2);
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.Property(x => x.Source).HasConversion<string>();
        builder.Property(x => x.Type).HasConversion<string>();
        builder.Property(x => x.Status).HasConversion<string>();

        builder.HasIndex(x => new { x.UnitId, x.StartDate, x.EndDate });

        builder.HasQueryFilter(x => !x.User.IsDeleted);
    }
}
