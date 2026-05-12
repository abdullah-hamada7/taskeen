using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Taskeen.Application.DTOs;
using Taskeen.Application.Interfaces;
using Taskeen.Domain.Entities;
using Taskeen.Domain.Enums;

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
    public async Task<ActionResult<BookingDto>> CreateOwnerBooking([FromBody] BookingRequest request)
    {
        int ownerId = GetCurrentUserId();
        var booking = await _bookingService.CreateOwnerBookingAsync(request.UnitId, ownerId, request.StartDate, request.EndDate);
        return Ok(MapToDto(booking));
    }

    [HttpPost("guest")]
    [Authorize(Roles = "Admin,Guest")]
    public async Task<ActionResult<BookingDto>> CreateGuestBooking([FromBody] GuestBookingRequest request)
    {
        var booking = await _bookingService.CreateGuestBookingAsync(request.UnitId, request.UserId, request.StartDate, request.EndDate, request.Source);
        return Ok(MapToDto(booking));
    }

    private BookingDto MapToDto(Booking booking)
    {
        return new BookingDto(
            booking.Id,
            booking.UnitId,
            booking.Unit?.Code ?? "Unknown",
            booking.UserId,
            booking.User?.FullName ?? "Unknown",
            booking.StartDate,
            booking.EndDate,
            booking.Source,
            booking.Type,
            booking.Status,
            booking.BaseAmount,
            booking.TaxAmount,
            booking.TotalAmount
        );
    }
}

public record BookingRequest(int UnitId, DateTime StartDate, DateTime EndDate);
public record GuestBookingRequest(int UnitId, int UserId, DateTime StartDate, DateTime EndDate, BookingSource Source);
