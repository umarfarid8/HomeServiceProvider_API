using System;

namespace HomeServiceProvider.Dtos.Availability;

public class AvailabilitySlotDto
{
    public Guid Id { get; set; }
    public string DayOfWeek { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public bool IsRecurring { get; set; }
    public DateTime? SpecificDate { get; set; }
    public bool IsAvailable { get; set; }
}