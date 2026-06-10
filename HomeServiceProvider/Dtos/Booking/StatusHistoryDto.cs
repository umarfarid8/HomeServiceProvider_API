using System;

namespace HomeServiceProvider.Dtos.Booking;

public class StatusHistoryDto
{
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime ChangedAt { get; set; }
}