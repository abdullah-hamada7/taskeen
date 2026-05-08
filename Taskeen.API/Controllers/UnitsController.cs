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
    public async Task<IActionResult> GetUnits()
    {
        var role = _currentUserService.Role;
        var userId = int.Parse(_currentUserService.UserId ?? "0");

        if (role == "Admin")
            return Ok(await _unitRepository.GetAllUnitsAsync());

        if (role == "Owner")
            return Ok(await _unitRepository.GetUnitsByOwnerIdAsync(userId));

        if (role == "Supervisor")
            return Ok(await _unitRepository.GetUnitsBySupervisorIdAsync(userId));

        return Forbid();
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetUnit(int id)
    {
        var unit = await _unitRepository.GetUnitByIdAsync(id);
        if (unit == null) return NotFound();

        var role = _currentUserService.Role;
        var userId = int.Parse(_currentUserService.UserId ?? "0");

        // Security check: Only Admin, the Owner, or the Supervisor can see unit details
        if (role != "Admin" && unit.OwnerId != userId && unit.SupervisorId != userId)
            return Forbid();

        return Ok(unit);
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

    [HttpGet("calendar/admin")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAdminCalendar()
    {
        var bookings = await _bookingRepository.GetAllBookingsAsync();
        var units = await _unitRepository.GetAllUnitsAsync();

        var reports = bookings.Select(b => new AdminReportDTO(
            units.FirstOrDefault(u => u.Id == b.UnitId)?.Code ?? "Unknown",
            b.BaseAmount,
            b.TaxAmount,
            b.TotalAmount
        )).ToList();

        return Ok(reports);
    }

    [HttpGet("calendar/owner/{id}")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> GetOwnerCalendar(int id)
    {
        var unit = await _unitRepository.GetUnitByIdAsync(id);
        if (unit == null) return NotFound();

        var userId = int.Parse(_currentUserService.UserId ?? "0");
        if (unit.OwnerId != userId) return Forbid();

        var bookings = await _bookingRepository.GetBookingsByUnitIdAsync(id);
        var calendar = bookings.Select(b => new OwnerCalendarDTO(
            unit.Code,
            b.StartDate,
            b.EndDate,
            b.BaseAmount
        )).ToList();

        return Ok(calendar);
    }
    [HttpGet("my-occupancy")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> GetOwnerOccupancy()
    {
        var ownerId = int.Parse(_currentUserService.UserId ?? "0");
        var units = await _unitRepository.GetUnitsByOwnerIdAsync(ownerId);
        
        var statuses = new List<object>();
        foreach (var unit in units)
        {
            var occupancy = await _unitRepository.GetOccupancyAsync(unit.Id, DateTime.UtcNow);
            statuses.Add(new
            {
                unit.Code,
                unit.TotalBeds,
                Occupancy = occupancy,
                IsFull = occupancy >= unit.TotalBeds
            });
        }

        return Ok(statuses);
    }
}
