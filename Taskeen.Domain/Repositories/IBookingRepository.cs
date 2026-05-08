using Taskeen.Domain.Entities;
using Taskeen.Domain.Enums;

namespace Taskeen.Domain.Repositories;

public interface IBookingRepository
{
    Task<int> GetOwnerUsedDaysAsync(int ownerId, int year);
    Task CreateBookingAsync(Booking booking);
    Task UpdateBookingStatusAsync(int bookingId, BookingStatus newStatus);
    Task<IEnumerable<Booking>> GetAllBookingsAsync();
    Task<IEnumerable<Booking>> GetBookingsByUnitIdAsync(int unitId);
    Task<Booking?> GetBookingByIdAsync(int id);
    Task UpdateBookingAsync(Booking booking);
}
