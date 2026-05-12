using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Taskeen.Application.DTOs;
using Taskeen.Application.Interfaces;
using Taskeen.Domain.Repositories;
using Taskeen.Domain.Entities;

namespace Taskeen.API.Controllers;

[ApiController]
[Route("api/units")]
public class UnitsController : ControllerBase
{
    private readonly IUnitRepository _unitRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly ICurrentUserService _currentUserService;

    public UnitsController(IUnitRepository unitRepository, IBookingRepository bookingRepository, ICurrentUserService currentUserService)
    {
        _unitRepository = unitRepository;
        _bookingRepository = bookingRepository;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<UnitDto>>> GetUnits()
    {
        var role = _currentUserService.Role;
        var userId = int.Parse(_currentUserService.UserId ?? "0");

        IEnumerable<Unit> units;
        if (role == "Admin")
            units = await _unitRepository.GetAllUnitsAsync();
        else if (role == "Owner")
            units = await _unitRepository.GetUnitsByOwnerIdAsync(userId);
        else if (role == "Supervisor")
            units = await _unitRepository.GetUnitsBySupervisorIdAsync(userId);
        else
            return Forbid();

        return Ok(units.Select(MapToDto));
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<UnitDto>> GetUnit(int id)
    {
        var unit = await _unitRepository.GetUnitByIdAsync(id);
        if (unit == null) return NotFound();

        var role = _currentUserService.Role;
        var userId = int.Parse(_currentUserService.UserId ?? "0");

        if (role != "Admin" && unit.OwnerId != userId && unit.SupervisorId != userId)
            return Forbid();

        return Ok(MapToDto(unit));
    }

    [HttpPost("{id}/beds")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddBed(int id, [FromBody] UnitBed bed)
    {
        var unit = await _unitRepository.GetUnitByIdAsync(id);
        if (unit == null) return NotFound();

        var currentBedsCount = await _unitRepository.GetTotalBedsCountAsync(id);
        if (currentBedsCount >= unit.TotalBeds)
            return BadRequest($"Unit {unit.Code} already has the maximum number of beds ({unit.TotalBeds}).");

        bed.UnitId = id;
        await _unitRepository.AddBedToUnitAsync(bed);
        return Ok(bed);
    }

    [HttpGet("my-occupancy")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> GetOwnerOccupancy()
    {
        var ownerId = int.Parse(_currentUserService.UserId ?? "0");
        var units = await _unitRepository.GetUnitsByOwnerIdAsync(ownerId);
        
        var statuses = new List<UnitStatusDto>();
        foreach (var unit in units)
        {
            var occupancy = await _unitRepository.GetOccupancyAsync(unit.Id, DateTime.UtcNow);
            statuses.Add(new UnitStatusDto(
                unit.Code,
                unit.TotalBeds,
                occupancy,
                occupancy >= unit.TotalBeds
            ));
        }

        return Ok(statuses);
    }

    private UnitDto MapToDto(Unit unit)
    {
        return new UnitDto(
            unit.Id,
            unit.Code,
            unit.BasePrice,
            unit.TotalBeds,
            unit.Tower != null ? new TowerDto(unit.Tower.Id, unit.Tower.Name) : null,
            unit.Owner != null ? new UserDto(unit.Owner.Id, unit.Owner.FullName, unit.Owner.IdentityNumber, unit.Owner.Nationality, unit.Owner.Role) : null,
            unit.Supervisor != null ? new UserDto(unit.Supervisor.Id, unit.Supervisor.FullName, unit.Supervisor.IdentityNumber, unit.Supervisor.Nationality, unit.Supervisor.Role) : null,
            unit.Beds?.Select(b => new UnitBedDto(b.Id, b.BedNumber)).ToList()
        );
    }
}
