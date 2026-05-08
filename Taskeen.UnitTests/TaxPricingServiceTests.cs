using Taskeen.Application.Services;
using Taskeen.Domain.Enums;
using Xunit;

namespace Taskeen.UnitTests;

public class TaxPricingServiceTests
{
    private readonly TaxPricingService _service;

    public TaxPricingServiceTests()
    {
        _service = new TaxPricingService();
    }

    [Fact]
    public void CalculateTaxAmount_WebsiteSource_Returns20Percent()
    {
        // Arrange
        decimal basePrice = 1000m;
        var source = BookingSource.Website;

        // Act
        var result = _service.CalculateTaxAmount(basePrice, source);

        // Assert
        Assert.Equal(200m, result);
    }

    [Fact]
    public void CalculateTaxAmount_CallCenterSource_ReturnsZero()
    {
        // Arrange
        decimal basePrice = 1000m;
        var source = BookingSource.CallCenter;

        // Act
        var result = _service.CalculateTaxAmount(basePrice, source);

        // Assert
        Assert.Equal(0m, result);
    }
}
