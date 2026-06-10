using System;
using System.Collections.Generic;

namespace HomeServiceProvider.Dtos.Booking;

public class BookingDto
{
    public Guid Id { get; set; }
    public Guid CustomerProfileId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid ProviderProfileId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public string ServiceCategory { get; set; } = string.Empty;
    public string ProblemDescription { get; set; } = string.Empty;
    public string ScheduledDate { get; set; } = string.Empty;
    public string ScheduledStartTime { get; set; } = string.Empty;
    public string ScheduledEndTime { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsEmergency { get; set; }
    public bool IsOffHours { get; set; }
    public decimal EstimatedAmount { get; set; }
    public decimal? FinalAmount { get; set; }
    public string? CancellationReason { get; set; }
    public Guid ChatThreadId { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<StatusHistoryDto> StatusHistory { get; set; } = new();
}