using Taskeen.Domain.Common;

namespace Taskeen.Domain.Entities;

public class UnitBed : AuditableEntity
{
    public int Id { get; set; }
    public int UnitId { get; set; }
    public int BedNumber { get; set; }
    public Unit Unit { get; set; } = null!;
}
