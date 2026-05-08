using Microsoft.EntityFrameworkCore;
using Taskeen.Domain.Entities;
using Taskeen.Domain.Common;
using Taskeen.Application.Interfaces;
using System.Reflection;

namespace Taskeen.Infrastructure.Persistence;

public class TaskeenDbContext : DbContext
{
    private readonly ICurrentUserService _currentUserService;

    public TaskeenDbContext(DbContextOptions<TaskeenDbContext> options, ICurrentUserService currentUserService) : base(options) 
    { 
        _currentUserService = currentUserService;
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Tower> Towers => Set<Tower>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<UnitBed> UnitBeds => Set<UnitBed>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var identity = _currentUserService.UserName ?? "System";
        var userIdString = _currentUserService.UserId ?? "0";
        int.TryParse(userIdString, out var userId);
        var now = DateTime.UtcNow;

        var auditEntries = new List<AuditEntry>();

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                continue;

            var auditEntry = new AuditEntry(entry)
            {
                EntityName = entry.Entity.GetType().Name,
                UserId = userId,
                Action = entry.State.ToString(),
                Timestamp = now
            };
            auditEntries.Add(auditEntry);

            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy = identity;
                    break;
                case EntityState.Modified:
                    entry.Entity.LastModifiedAt = now;
                    entry.Entity.LastModifiedBy = identity;
                    break;
            }
        }

        foreach (var auditEntry in auditEntries)
        {
            AuditLogs.Add(auditEntry.ToAuditLog());
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}

public class AuditEntry
{
    public AuditEntry(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
    {
        Entry = entry;
    }

    public Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry Entry { get; }
    public string EntityName { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object> OldValues { get; } = new();
    public Dictionary<string, object> NewValues { get; } = new();

    public AuditLog ToAuditLog()
    {
        foreach (var property in Entry.Properties)
        {
            string propertyName = property.Metadata.Name;
            if (property.Metadata.IsPrimaryKey())
            {
                continue;
            }

            switch (Entry.State)
            {
                case EntityState.Added:
                    NewValues[propertyName] = property.CurrentValue!;
                    break;
                case EntityState.Deleted:
                    OldValues[propertyName] = property.OriginalValue!;
                    break;
                case EntityState.Modified:
                    if (property.IsModified)
                    {
                        OldValues[propertyName] = property.OriginalValue!;
                        NewValues[propertyName] = property.CurrentValue!;
                    }
                    break;
            }
        }

        return new AuditLog
        {
            UserId = UserId,
            Action = Action,
            EntityName = EntityName,
            EntityId = (int)Entry.Property("Id").CurrentValue!,
            Timestamp = Timestamp,
            OldValues = System.Text.Json.JsonSerializer.Serialize(OldValues),
            NewValues = System.Text.Json.JsonSerializer.Serialize(NewValues)
        };
    }
}
