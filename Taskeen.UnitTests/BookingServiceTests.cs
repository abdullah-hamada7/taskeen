using Moq;
using Taskeen.Application.Services;
using Taskeen.Application.Interfaces;
using Taskeen.Domain.Entities;
using Taskeen.Domain.Enums;
using Taskeen.Domain.Repositories;
using Xunit;

namespace Taskeen.UnitTests;

public class BookingServiceTests
{
    private readonly Mock<IBookingRepository> _bookingRepoMock;
    private readonly Mock<IUnitRepository> _unitRepoMock;
    private readonly Mock<ITaxPricingService> _taxServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly BookingService _service;

    public BookingServiceTests()
    {
        _bookingRepoMock = new Mock<IBookingRepository>();
        _unitRepoMock = new Mock<IUnitRepository>();
        _taxServiceMock = new Mock<ITaxPricingService>();
        _currentUserMock = new Mock<ICurrentUserService>();
        
        _currentUserMock.Setup(x => x.UserId).Returns("1");

        _service = new BookingService(_bookingRepoMock.Object, _unitRepoMock.Object, _taxServiceMock.Object, _currentUserMock.Object);
    }

    [Fact]
    public async Task CreateOwnerBookingAsync_Exceeds15Days_ThrowsException()
    {
        // Arrange
        int ownerId = 1;
        int unitId = 101;
        DateTime start = new DateTime(2026, 5, 1);
        DateTime end = new DateTime(2026, 5, 10); // 9 days
        
        _unitRepoMock.Setup(x => x.GetUnitByIdAsync(unitId))
            .ReturnsAsync(new Unit { Id = unitId, BasePrice = 100 });

        _bookingRepoMock.Setup(x => x.GetOwnerUsedDaysAsync(ownerId, 2026))
            .ReturnsAsync(7); // 7 + 9 = 16 > 15

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _service.CreateOwnerBookingAsync(unitId, ownerId, start, end));
    }

    [Fact]
    public async Task CreateOwnerBookingAsync_WithinLimit_Succeeds()
    {
        // Arrange
        int ownerId = 1;
        int unitId = 101;
        DateTime start = new DateTime(2026, 5, 1);
        DateTime end = new DateTime(2026, 5, 5); // 4 days
        
        _unitRepoMock.Setup(x => x.GetUnitByIdAsync(unitId))
            .ReturnsAsync(new Unit { Id = unitId, BasePrice = 100 });

        _bookingRepoMock.Setup(x => x.GetOwnerUsedDaysAsync(ownerId, 2026))
            .ReturnsAsync(5); // 5 + 4 = 9 <= 15


        // Act
        var result = await _service.CreateOwnerBookingAsync(unitId, ownerId, start, end);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(BookingType.OwnerPersonal, result.Type);
        _bookingRepoMock.Verify(x => x.CreateBookingAsync(It.IsAny<Booking>()), Times.Once);
    }
}
