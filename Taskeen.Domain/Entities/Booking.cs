using System.ComponentModel.DataAnnotations;
using Taskeen.Domain.Common;
using Taskeen.Domain.Enums;

namespace Taskeen.Domain.Entities;

public class Booking : AuditableEntity
{
    public int Id { get; set; }
    public int UnitId { get; set; }
    public Unit Unit { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int? BedId { get; set; }
    public UnitBed? Bed { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public BookingSource Source { get; set; }
    public BookingType Type { get; set; }
    public BookingStatus Status { get; set; }

    public decimal BaseAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; } = null!;
}
