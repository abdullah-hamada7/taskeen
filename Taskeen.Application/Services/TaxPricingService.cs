using Taskeen.Application.Interfaces;
using Taskeen.Domain.Enums;

namespace Taskeen.Application.Services;

public class TaxPricingService : ITaxPricingService
{
    public decimal CalculateTaxAmount(decimal basePrice, BookingSource source)
    {
        // 20% tax if Source == Website
        if (source == BookingSource.Website)
        {
            return basePrice * 0.20m;
        }
        return 0;
    }
}
