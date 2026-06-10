using System;
using System.Collections.Generic;

namespace HomeServiceProvider.Dtos.Availability;

public class ProviderAvailabilityResponseDto
{
    public Guid ProviderProfileId { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string DayOfWeek { get; set; } = string.Empty;
    public bool IsDayAvailable { get; set; }
    public string? WorkingHoursStart { get; set; }
    public string? WorkingHoursEnd { get; set; }
}

