using System;
using System.ComponentModel.DataAnnotations;

namespace HomeServiceProvider.Dtos.Availability;

public class WeeklySlotDto
{
    [Required]
    public DayOfWeek DayOfWeek { get; set; }

    [Required]
    public TimeOnly StartTime { get; set; }

    [Required]
    public TimeOnly EndTime { get; set; }
}