using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Taskeen.Application.DTOs;
using Taskeen.Application.Interfaces;
using Taskeen.Domain.Repositories;
using Taskeen.Domain.Entities;
using Taskeen.Domain.Enums;

namespace Taskeen.API.Controllers;

[ApiController]
[Route("api/supervisors")]
[Authorize(Roles = "Supervisor")]
public class SupervisorController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly IUnitRepository _unitRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserRepository _userRepository;

    public SupervisorController(IBookingService bookingService, IUnitRepository unitRepository, ICurrentUserService currentUserService, IUserRepository userRepository)
    {
        _bookingService = bookingService;
        _unitRepository = unitRepository;
        _currentUserService = currentUserService;
        _userRepository = userRepository;
    }

    private int GetCurrentUserId()
    {
        if (int.TryParse(_currentUserService.UserId, out var id))
            return id;
        throw new UnauthorizedAccessException("Invalid user identity.");
    }

    [HttpPost("register-guest")]
    public async Task<IActionResult> RegisterGuest([FromBody] TenantRegistrationDTO registration)
    {
        var user = new User
        {
            IdentityNumber = registration.IdentityNumber,
            FullName = registration.FullName,
            Nationality = registration.Nationality,
            Role = UserRole.Guest
        };

        await _userRepository.RegisterRoleAsync(user);
        return Ok(new UserDto(user.Id, user.FullName, user.IdentityNumber, user.Nationality, user.Role));
    }

    [HttpPost("check-in")]
    public async Task<IActionResult> CheckIn([FromBody] CheckInRequest request)
    {
        await _bookingService.CheckInGuestAsync(request.BookingId, request.BedId, GetCurrentUserId());
        return Ok();
    }

    [HttpGet("units")]
    public async Task<ActionResult<IEnumerable<SupervisorUnitDto>>> GetAssignedUnits()
    {
        var units = await _unitRepository.GetUnitsBySupervisorIdAsync(GetCurrentUserId());
        
        var statuses = new List<SupervisorUnitDto>();
        foreach (var unit in units)
        {
            var occupants = await _unitRepository.GetOccupantNamesAsync(unit.Id, DateTime.UtcNow);
            statuses.Add(new SupervisorUnitDto(unit.Code, unit.TotalBeds, occupants));
        }

        return Ok(statuses);
    }

    [HttpDelete("check-out/{id}")]
    public async Task<IActionResult> CheckOut(int id)
    {
        await _bookingService.CheckOutGuestAsync(id, GetCurrentUserId());
        return NoContent();
    }
}

public record CheckInRequest(int BookingId, int BedId);
