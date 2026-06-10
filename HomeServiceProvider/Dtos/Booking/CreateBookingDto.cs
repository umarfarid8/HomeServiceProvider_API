using System;
using System.ComponentModel.DataAnnotations;

namespace HomeServiceProvider.Dtos.Booking;

public class CreateBookingDto
{
    [Required]
    public Guid ProviderProfileId { get; set; }

    [Required]
    public Guid ServiceCategoryId { get; set; }

    public Guid? MatchRequestId { get; set; }

    [Required, MaxLength(1000)]
    public string ProblemDescription { get; set; } = string.Empty;

    [Required]
    public DateTime ScheduledDate { get; set; }

    [Required]
    public TimeOnly ScheduledStartTime { get; set; }

    [Required]
    public TimeOnly ScheduledEndTime { get; set; }

    public bool IsEmergency { get; set; } = false;
}