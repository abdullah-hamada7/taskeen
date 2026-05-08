using Taskeen.Domain.Entities;
using Taskeen.Domain.Enums;

namespace Taskeen.Application.Interfaces;

public interface IBookingService
{
    Task<Booking> CreateOwnerBookingAsync(int unitId, int ownerId, DateTime start, DateTime end);
    Task<Booking> CreateGuestBookingAsync(int unitId, int guestId, DateTime start, DateTime end, BookingSource source);
    Task CheckInGuestAsync(int bookingId, int bedId, int supervisorId);
    Task CheckOutGuestAsync(int bookingId, int supervisorId);
}
