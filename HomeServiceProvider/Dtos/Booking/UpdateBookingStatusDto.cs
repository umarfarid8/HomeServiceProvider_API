using System.ComponentModel.DataAnnotations;
using HomeServiceProvider.DataAccess.Enums;

namespace HomeServiceProvider.Dtos.Booking;

public class UpdateBookingStatusDto
{
    [Required]
    public BookingStatus NewStatus { get; set; }
    public string? Notes { get; set; }
    public string? CancellationReason { get; set; }
}