using Taskeen.Application.Interfaces;
using Taskeen.Domain.Entities;
using Taskeen.Domain.Enums;
using Taskeen.Domain.Repositories;

namespace Taskeen.Application.Services;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IUnitRepository _unitRepository;
    private readonly ITaxPricingService _taxPricingService;
    private readonly ICurrentUserService _currentUserService;

    public BookingService(
        IBookingRepository bookingRepository,
        IUnitRepository unitRepository,
        ITaxPricingService taxPricingService,
        ICurrentUserService currentUserService)
    {
        _bookingRepository = bookingRepository;
        _unitRepository = unitRepository;
        _taxPricingService = taxPricingService;
        _currentUserService = currentUserService;
    }

    public async Task<Booking> CreateOwnerBookingAsync(int unitId, int ownerId, DateTime start, DateTime end)
    {
        var unit = await _unitRepository.GetUnitByIdAsync(unitId);
        if (unit == null) throw new ArgumentException("Unit not found.");

        if (unit.OwnerId != ownerId)
            throw new UnauthorizedAccessException("You can only book personal stay for units you own.");

        // Validate 15-day annual limit
        int currentYear = start.Year;
        int usedDays = await _bookingRepository.GetOwnerUsedDaysAsync(ownerId, currentYear);
        int requestedDays = (end - start).Days;

        if (usedDays + requestedDays > 15)
        {
            throw new InvalidOperationException("Owner personal stay limit exceeded (15 days annually).");
        }

        if (!await _unitRepository.HasAvailableCapacityAsync(unitId, start, end))
        {
            throw new InvalidOperationException("Unit is fully booked for the selected period.");
        }

        var booking = new Booking
        {
            UnitId = unitId,
            UserId = ownerId,
            StartDate = start,
            EndDate = end,
            Source = BookingSource.CallCenter, // Implicit for owner bookings
            Type = BookingType.OwnerPersonal,
            Status = BookingStatus.Confirmed,
            BaseAmount = unit.BasePrice, // Record market value for reporting
            TaxAmount = 0, // Owners usually don't pay tax on personal stay
            TotalAmount = 0 // But they pay 0 total
        };

        await _bookingRepository.CreateBookingAsync(booking);
        return booking;
    }

    public async Task<Booking> CreateGuestBookingAsync(int unitId, int guestId, DateTime start, DateTime end, BookingSource source)
    {
        var unit = await _unitRepository.GetUnitByIdAsync(unitId);
        if (unit == null) throw new ArgumentException("Unit not found.");

        decimal baseAmount = unit.BasePrice * (end - start).Days;
        decimal taxAmount = _taxPricingService.CalculateTaxAmount(baseAmount, source);
        decimal totalAmount = baseAmount + taxAmount;

        if (!await _unitRepository.HasAvailableCapacityAsync(unitId, start, end))
        {
            throw new InvalidOperationException("Unit is fully booked for the selected period.");
        }

        var booking = new Booking
        {
            UnitId = unitId,
            UserId = guestId,
            StartDate = start,
            EndDate = end,
            Source = source,
            Type = BookingType.GuestBooking,
            Status = BookingStatus.Confirmed,
            BaseAmount = baseAmount,
            TaxAmount = taxAmount,
            TotalAmount = totalAmount
        };

        await _bookingRepository.CreateBookingAsync(booking);
        return booking;
    }

    public async Task CheckInGuestAsync(int bookingId, int bedId, int supervisorId)
    {
        var booking = await _bookingRepository.GetBookingByIdAsync(bookingId);
        if (booking == null) throw new ArgumentException("Booking not found.");
        if (booking.Status != BookingStatus.Confirmed && booking.Status != BookingStatus.Pending)
            throw new InvalidOperationException("Booking is not in a state that allows check-in.");

        var unit = await _unitRepository.GetUnitByIdAsync(booking.UnitId);
        if (unit == null) throw new InvalidOperationException("Unit not found.");

        if (_currentUserService.Role != "Admin" && unit.SupervisorId != supervisorId)
            throw new UnauthorizedAccessException("You are not authorized to check-in guests for this unit.");

        // Check if bed is available
        var availableBeds = await _unitRepository.GetAvailableBedsAsync(booking.UnitId, booking.StartDate, booking.EndDate);
        if (!availableBeds.Any(b => b.Id == bedId))
            throw new InvalidOperationException("Selected bed is not available for this period.");

        booking.Status = BookingStatus.CheckedIn;
        booking.BedId = bedId;

        await _bookingRepository.UpdateBookingAsync(booking);
    }

    public async Task CheckOutGuestAsync(int bookingId, int supervisorId)
    {
        var booking = await _bookingRepository.GetBookingByIdAsync(bookingId);
        if (booking == null) throw new ArgumentException("Booking not found.");
        if (booking.Status != BookingStatus.CheckedIn)
            throw new InvalidOperationException("Booking is not checked in.");

        var unit = await _unitRepository.GetUnitByIdAsync(booking.UnitId);
        if (unit == null) throw new InvalidOperationException("Unit not found.");

        if (_currentUserService.Role != "Admin" && unit.SupervisorId != supervisorId)
            throw new UnauthorizedAccessException("You are not authorized to check-out guests for this unit.");

        booking.Status = BookingStatus.CheckedOut;

        await _bookingRepository.UpdateBookingAsync(booking);
    }
}
