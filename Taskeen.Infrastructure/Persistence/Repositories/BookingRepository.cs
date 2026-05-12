using Microsoft.EntityFrameworkCore;
using Taskeen.Domain.Entities;
using Taskeen.Domain.Enums;
using Taskeen.Domain.Repositories;

namespace Taskeen.Infrastructure.Persistence.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly TaskeenDbContext _context;

    public BookingRepository(TaskeenDbContext context)
    {
        _context = context;
    }

    public async Task<int> GetOwnerUsedDaysAsync(int ownerId, int year)
    {
        return await _context.Bookings
            .Where(b => b.UserId == ownerId && 
                        b.Type == BookingType.OwnerPersonal && 
                        b.StartDate.Year == year &&
                        b.Status != BookingStatus.Cancelled)
            .SumAsync(b => EF.Functions.DateDiffDay(b.StartDate, b.EndDate));
    }

    public async Task CreateBookingAsync(Booking booking)
    {
        await _context.Bookings.AddAsync(booking);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateBookingStatusAsync(int bookingId, BookingStatus newStatus)
    {
        var booking = await _context.Bookings.FindAsync(bookingId);
        if (booking != null)
        {
            booking.Status = newStatus;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Booking>> GetAllBookingsAsync()
    {
        return await _context.Bookings.Include(b => b.Unit).Include(b => b.User).ToListAsync();
    }

    public async Task<IEnumerable<Booking>> GetBookingsByUnitIdAsync(int unitId)
    {
        return await _context.Bookings.Include(b => b.Unit).Include(b => b.User).Where(b => b.UnitId == unitId).ToListAsync();
    }

    public async Task<Booking?> GetBookingByIdAsync(int id)
    {
        return await _context.Bookings.Include(b => b.Unit).Include(b => b.User).FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task UpdateBookingAsync(Booking booking)
    {
        _context.Bookings.Update(booking);
        await _context.SaveChangesAsync();
    }
}
