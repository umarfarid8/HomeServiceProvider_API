using HomeServiceProvider.DataAccess.Enums;
using HomeServiceProvider.Dtos.Pricing;
using HomeServiceProvider.Services.Interfaces;
using HomeServiceProvider.UnitOfWork;

namespace HomeServiceProvider.Services;

public class PricingService : IPricingService
{
    private static readonly TimeOnly BusinessStart = new(9, 0);
    private static readonly TimeOnly BusinessEnd = new(18, 0);
    private readonly IUnitOfWork _uow;

    public PricingService(IUnitOfWork uow) => _uow = uow;

    public async Task<PricingBreakdownDto> CalculatePriceAsync(Guid providerProfileId, Guid serviceCategoryId, DateTime scheduledDate, TimeOnly startTime, TimeOnly endTime, bool isEmergency)
    {
        if (startTime >= endTime)
            throw new InvalidOperationException("Start time must be before end time.");

        var providerService = await _uow.ProviderServices.FirstOrDefaultAsync(ps =>
            ps.ProviderProfileId == providerProfileId &&
            ps.ServiceCategoryId == serviceCategoryId);

        decimal hourlyRate;
        if (providerService is not null)
        {
            hourlyRate = providerService.HourlyRate;
        }
        else
        {
            var profile = await _uow.ProviderProfiles.GetByIdAsync(providerProfileId)
                ?? throw new KeyNotFoundException("Provider not found.");
            hourlyRate = profile.BaseHourlyRate;
        }

        var duration = (endTime.ToTimeSpan() - startTime.ToTimeSpan()).TotalHours;
        decimal baseAmount = hourlyRate * (decimal)duration;

        bool isOffHours = startTime < BusinessStart || startTime >= BusinessEnd;
        bool isWeekend = scheduledDate.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

        var rules = await _uow.PricingRules.FindAsync(r => r.IsActive);
        decimal totalMultiplier = 1.0m;
        var appliedSurcharges = new List<string>();

        if (isEmergency)
        {
            var rule = rules.FirstOrDefault(r => r.RuleType == PricingRuleType.Emergency);
            if (rule is not null)
            {
                totalMultiplier *= rule.Multiplier;
                appliedSurcharges.Add($"{rule.RuleName} (×{rule.Multiplier:F2})");
            }
        }
        if (isOffHours)
        {
            var rule = rules.FirstOrDefault(r => r.RuleType == PricingRuleType.OffHours);
            if (rule is not null)
            {
                totalMultiplier *= rule.Multiplier;
                appliedSurcharges.Add($"{rule.RuleName} (×{rule.Multiplier:F2})");
            }
        }
        if (isWeekend)
        {
            var rule = rules.FirstOrDefault(r => r.RuleType == PricingRuleType.Weekend);
            if (rule is not null)
            {
                totalMultiplier *= rule.Multiplier;
                appliedSurcharges.Add($"{rule.RuleName} (×{rule.Multiplier:F2})");
            }
        }

        decimal finalAmount = Math.Round(baseAmount * totalMultiplier, 2);

        return new PricingBreakdownDto
        {
            HourlyRate = hourlyRate,
            DurationHours = Math.Round((decimal)duration, 2),
            BaseAmount = Math.Round(baseAmount, 2),
            TotalMultiplier = totalMultiplier,
            FinalAmount = finalAmount,
            IsEmergency = isEmergency,
            IsOffHours = isOffHours,
            IsWeekend = isWeekend,
            AppliedSurcharges = appliedSurcharges
        };
    }
}