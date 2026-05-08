using System.ComponentModel.DataAnnotations;
using Taskeen.Domain.Common;

namespace Taskeen.Domain.Entities;

public class Unit : AuditableEntity
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public int TotalBeds { get; set; }

    public int TowerId { get; set; }
    public Tower Tower { get; set; } = null!;

    public int OwnerId { get; set; }
    public User Owner { get; set; } = null!;

    public int SupervisorId { get; set; }
    public User Supervisor { get; set; } = null!;

    [Timestamp]
    public byte[] RowVersion { get; set; } = null!;

    public ICollection<UnitBed> Beds { get; set; } = new List<UnitBed>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
