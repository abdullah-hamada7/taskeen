using Taskeen.Domain.Enums;

namespace Taskeen.Application.Interfaces;

public interface ITaxPricingService
{
    decimal CalculateTaxAmount(decimal basePrice, BookingSource source);
}
