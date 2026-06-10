using System.Collections.Generic;

namespace HomeServiceProvider.Dtos.Pricing;

public class PricingBreakdownDto
{
    public decimal HourlyRate { get; set; }
    public decimal DurationHours { get; set; }
    public decimal BaseAmount { get; set; }
    public decimal TotalMultiplier { get; set; }
    public decimal FinalAmount { get; set; }
    public bool IsEmergency { get; set; }
    public bool IsOffHours { get; set; }
    public bool IsWeekend { get; set; }
    public List<string> AppliedSurcharges { get; set; } = new();
}