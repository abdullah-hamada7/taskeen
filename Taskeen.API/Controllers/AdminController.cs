using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Taskeen.Application.DTOs;
using Taskeen.Application.Interfaces;
using Taskeen.Domain.Entities;
using Taskeen.Domain.Repositories;

namespace Taskeen.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IUnitRepository _unitRepository;
    private readonly IUserRepository _userRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly ICurrentUserService _currentUserService;

    public AdminController(IUnitRepository unitRepository, IUserRepository userRepository, IBookingRepository bookingRepository, ICurrentUserService currentUserService)
    {
        _unitRepository = unitRepository;
        _userRepository = userRepository;
        _bookingRepository = bookingRepository;
        _currentUserService = currentUserService;
    }

    private string GetCurrentUserId()
    {
        return _currentUserService.UserId ?? throw new UnauthorizedAccessException("Invalid user identity.");
    }

    [HttpGet("reports/financial")]
    public async Task<IActionResult> GetFinancialReport()
    {
        var bookings = await _bookingRepository.GetAllBookingsAsync();
        var units = await _unitRepository.GetAllUnitsAsync();
        
        var report = bookings
            .Join(units, b => b.UnitId, u => u.Id, (b, u) => new { b, u })
            .GroupBy(x => new { x.u.Tower, Month = x.b.StartDate.ToString("yyyy-MM") })
            .Select(g => new
            {
                Tower = g.Key.Tower,
                Month = g.Key.Month,
                TotalRevenue = g.Sum(x => x.b.TotalAmount),
                TotalTax = g.Sum(x => x.b.TaxAmount),
                TotalBase = g.Sum(x => x.b.BaseAmount)
            })
            .OrderBy(x => x.Month).ThenBy(x => x.Tower)
            .ToList();

        return Ok(report);
    }

    [HttpPost("towers")]
    public async Task<IActionResult> CreateTower([FromBody] Tower tower)
    {
        await _unitRepository.CreateTowerAsync(tower);
        return Ok(tower);
    }

    [HttpPost("users")]
    [AllowAnonymous] // Added temporarily to allow you to create your first admin!
    public async Task<IActionResult> CreateUser([FromBody] User user)
    {
        await _userRepository.RegisterRoleAsync(user);
        return Ok(user);
    }

    [HttpPost("units")]
    public async Task<IActionResult> CreateUnit([FromBody] Unit unit)
    {
        await _unitRepository.CreateUnitAsync(unit);
        return Ok(unit);
    }

    [HttpPut("units/{id}/supervisor")]
    public async Task<IActionResult> AssignSupervisor(int id, [FromBody] int supervisorId)
    {
        await _unitRepository.AssignSupervisorAsync(id, supervisorId);
        return Ok();
    }

    [HttpGet("units/statuses")]
    public async Task<IActionResult> GetUnitStatuses()
    {
        var units = await _unitRepository.GetAllUnitsAsync();
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

    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        await _userRepository.SoftDeleteWithHistoryAsync(id);
        return NoContent();
    }
}
