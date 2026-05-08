using Taskeen.Domain.Common;

namespace Taskeen.Domain.Entities;

public class Tower : AuditableEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<Unit> Units { get; set; } = new List<Unit>();
}
