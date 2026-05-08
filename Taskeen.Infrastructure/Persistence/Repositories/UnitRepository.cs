using Microsoft.EntityFrameworkCore;
using Taskeen.Domain.Entities;
using Taskeen.Domain.Enums;
using Taskeen.Domain.Repositories;

namespace Taskeen.Infrastructure.Persistence.Repositories;

public class UnitRepository : IUnitRepository
{
    private readonly TaskeenDbContext _context;

    public UnitRepository(TaskeenDbContext context)
    {
        _context = context;
    }

    public async Task<Unit?> GetUnitByIdAsync(int id)
    {
        return await _context.Units.FindAsync(id);
    }

    public async Task CreateUnitAsync(Unit unit)
    {
        var owner = await _context.Users.FindAsync(unit.OwnerId);
        if (owner == null || owner.Role != UserRole.Owner)
            throw new InvalidOperationException("Assigned owner must exist and have the Owner role.");

        await _context.Units.AddAsync(unit);
        await _context.SaveChangesAsync();
    }

    public async Task AssignTowerAsync(int unitId, int towerId)
    {
        var unit = await _context.Units.FindAsync(unitId);
        if (unit != null)
        {
            unit.TowerId = towerId;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<UnitBed>> GetAvailableBedsAsync(int unitId, DateTime start, DateTime end)
    {
        // Get all beds for unit
        var allBeds = await _context.UnitBeds
            .Where(b => b.UnitId == unitId)
            .ToListAsync();

        // Get occupied bed IDs during the requested period
        var occupiedBedIds = await _context.Bookings
            .Where(b => b.UnitId == unitId && 
                        b.StartDate < end && 
                        b.EndDate > start && 
                        b.BedId.HasValue)
            .Select(b => b.BedId!.Value)
            .ToListAsync();

        return allBeds.Where(b => !occupiedBedIds.Contains(b.Id));
    }

    public async Task<int> GetOccupancyAsync(int unitId, DateTime date)
    {
        return await _context.Bookings
            .Where(b => b.UnitId == unitId &&
                        (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.CheckedIn) &&
                        date >= b.StartDate && date <= b.EndDate)
            .CountAsync();
    }

    public async Task<IEnumerable<Unit>> GetAllUnitsAsync()
    {
        return await _context.Units.ToListAsync();
    }

    public async Task<IEnumerable<Unit>> GetUnitsByOwnerIdAsync(int ownerId)
    {
        return await _context.Units.Where(u => u.OwnerId == ownerId).ToListAsync();
    }

    public async Task<IEnumerable<Unit>> GetUnitsBySupervisorIdAsync(int supervisorId)
    {
        return await _context.Units.Where(u => u.SupervisorId == supervisorId).ToListAsync();
    }

    public async Task AssignSupervisorAsync(int unitId, int supervisorId)
    {
        var unit = await _context.Units.FindAsync(unitId);
        var supervisor = await _context.Users.FindAsync(supervisorId);

        if (unit != null && supervisor != null)
        {
            if (supervisor.Role != UserRole.Supervisor)
                throw new InvalidOperationException("Assigned user must have the Supervisor role.");

            unit.SupervisorId = supervisorId;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<string>> GetOccupantNamesAsync(int unitId, DateTime date)
    {
        return await _context.Bookings
            .Include(b => b.User)
            .Where(b => b.UnitId == unitId &&
                        (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.CheckedIn) &&
                        date >= b.StartDate && date <= b.EndDate)
            .Select(b => b.User.FullName)
            .ToListAsync();
    }

    public async Task CreateTowerAsync(Tower tower)
    {
        await _context.Towers.AddAsync(tower);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Tower>> GetAllTowersAsync()
    {
        return await _context.Towers.ToListAsync();
    }

    public async Task<Tower?> GetTowerByIdAsync(int id)
    {
        return await _context.Towers.FindAsync(id);
    }

    public async Task AddBedToUnitAsync(UnitBed bed)
    {
        await _context.UnitBeds.AddAsync(bed);
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetTotalBedsCountAsync(int unitId)
    {
        return await _context.UnitBeds.CountAsync(b => b.UnitId == unitId);
    }

    public async Task<bool> HasAvailableCapacityAsync(int unitId, DateTime start, DateTime end)
    {
        var unit = await _context.Units.FindAsync(unitId);
        if (unit == null) return false;

        var activeBookingsCount = await _context.Bookings
            .CountAsync(b => b.UnitId == unitId &&
                             b.StartDate < end &&
                             b.EndDate > start &&
                             b.Status != BookingStatus.Cancelled);

        return activeBookingsCount < unit.TotalBeds;
    }
}
