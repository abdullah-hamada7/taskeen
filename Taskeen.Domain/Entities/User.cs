using Taskeen.Domain.Common;
using Taskeen.Domain.Enums;

namespace Taskeen.Domain.Entities;

public class User : AuditableEntity
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string IdentityNumber { get; set; } = string.Empty;
    public string Nationality { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsDeleted { get; set; }

    public ICollection<Unit> OwnedUnits { get; set; } = new List<Unit>();
    public ICollection<Unit> SupervisedUnits { get; set; } = new List<Unit>();
}
