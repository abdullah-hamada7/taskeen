using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Taskeen.Application.DTOs;
using Taskeen.Application.Interfaces;
using Taskeen.Domain.Enums;
using Taskeen.Domain.Repositories;

namespace Taskeen.API.Controllers;

[ApiController]
[Route("api/bookings")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly ICurrentUserService _currentUserService;

    public BookingsController(IBookingService bookingService, ICurrentUserService currentUserService)
    {
        _bookingService = bookingService;
        _currentUserService = currentUserService;
    }

    private int GetCurrentUserId()
    {
        if (int.TryParse(_currentUserService.UserId, out var id))
            return id;
        throw new UnauthorizedAccessException("Invalid user identity.");
    }

    [HttpPost("owner")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> CreateOwnerBooking([FromBody] BookingRequest request)
    {
        int ownerId = GetCurrentUserId();
        var booking = await _bookingService.CreateOwnerBookingAsync(request.UnitId, ownerId, request.StartDate, request.EndDate);
        return Ok(booking);
    }

    [HttpPost("guest")]
    [Authorize(Roles = "Admin,Guest")]
    public async Task<IActionResult> CreateGuestBooking([FromBody] GuestBookingRequest request)
    {
        var booking = await _bookingService.CreateGuestBookingAsync(request.UnitId, request.UserId, request.StartDate, request.EndDate, request.Source);
        return Ok(booking);
    }
}

public record BookingRequest(int UnitId, int UserId, DateTime StartDate, DateTime EndDate);
public record GuestBookingRequest(int UnitId, int UserId, DateTime StartDate, DateTime EndDate, BookingSource Source);
