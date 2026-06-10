using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HomeServiceProvider.Dtos.Availability;

public class SetWeeklyScheduleDto
{
    // The explicit <WeeklySlotDto> type parameter makes the Slots collection visible
    [Required, MinLength(1)]
    public List<WeeklySlotDto> Slots { get; set; } = new();
}