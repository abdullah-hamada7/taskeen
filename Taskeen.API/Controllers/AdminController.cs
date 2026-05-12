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

    [HttpGet("reports/financial")]
    public async Task<ActionResult<IEnumerable<FinancialReportDto>>> GetFinancialReport()
    {
        var bookings = await _bookingRepository.GetAllBookingsAsync();
        var units = await _unitRepository.GetAllUnitsAsync();
        
        var report = bookings
            .Join(units, b => b.UnitId, u => u.Id, (b, u) => new { b, u })
            .GroupBy(x => new { TowerName = x.u.Tower.Name, Month = x.b.StartDate.ToString("yyyy-MM") })
            .Select(g => new FinancialReportDto(
                g.Key.TowerName,
                g.Key.Month,
                g.Sum(x => x.b.TotalAmount),
                g.Sum(x => x.b.TaxAmount),
                g.Sum(x => x.b.BaseAmount)
            ))
            .OrderBy(x => x.Month).ThenBy(x => x.Tower)
            .ToList();

        return Ok(report);
    }

    [HttpGet("users")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        var users = await _userRepository.GetAllUsersAsync();
        return Ok(users.Select(u => new UserDto(u.Id, u.FullName, u.IdentityNumber, u.Nationality, u.Role)));
    }

    [HttpPost("towers")]
    public async Task<IActionResult> CreateTower([FromBody] Tower tower)
    {
        await _unitRepository.CreateTowerAsync(tower);
        return Ok(tower);
    }

    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        var user = new User
        {
            FullName = dto.FullName,
            IdentityNumber = dto.IdentityNumber,
            Nationality = dto.Nationality,
            Role = dto.Role,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            CreatedBy = _currentUserService.UserName ?? "System",
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.RegisterRoleAsync(user);
        return Ok(new UserDto(user.Id, user.FullName, user.IdentityNumber, user.Nationality, user.Role));
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
    public async Task<ActionResult<IEnumerable<UnitStatusDto>>> GetUnitStatuses()
    {
        var units = await _unitRepository.GetAllUnitsAsync();
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

    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        await _userRepository.SoftDeleteWithHistoryAsync(id);
        return NoContent();
    }
}
