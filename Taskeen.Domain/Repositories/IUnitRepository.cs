using Taskeen.Domain.Entities;

namespace Taskeen.Domain.Repositories;

public interface IUnitRepository
{
    Task<Unit?> GetUnitByIdAsync(int id);
    Task CreateUnitAsync(Unit unit);
    Task AssignTowerAsync(int unitId, int towerId);
    Task<IEnumerable<UnitBed>> GetAvailableBedsAsync(int unitId, DateTime start, DateTime end);
    Task<int> GetOccupancyAsync(int unitId, DateTime date);
    Task<IEnumerable<Unit>> GetAllUnitsAsync();
    Task<IEnumerable<Unit>> GetUnitsByOwnerIdAsync(int ownerId);
    Task<IEnumerable<Unit>> GetUnitsBySupervisorIdAsync(int supervisorId);
    Task AssignSupervisorAsync(int unitId, int supervisorId);
    Task<List<string>> GetOccupantNamesAsync(int unitId, DateTime date);
    Task CreateTowerAsync(Tower tower);
    Task<IEnumerable<Tower>> GetAllTowersAsync();
    Task<Tower?> GetTowerByIdAsync(int id);
    Task AddBedToUnitAsync(UnitBed bed);
    Task<int> GetTotalBedsCountAsync(int unitId);
    Task<bool> HasAvailableCapacityAsync(int unitId, DateTime start, DateTime end);
}