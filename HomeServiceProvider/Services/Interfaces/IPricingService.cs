using System;
using System.Threading.Tasks;
using HomeServiceProvider.Dtos.Pricing;

namespace HomeServiceProvider.Services.Interfaces;

public interface IPricingService
{
    Task<PricingBreakdownDto> CalculatePriceAsync(Guid providerProfileId, Guid serviceCategoryId, DateTime scheduledDate, TimeOnly startTime, TimeOnly endTime, bool isEmergency);
}