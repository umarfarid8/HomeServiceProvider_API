using System.ComponentModel.DataAnnotations;
using HomeServiceProvider.DataAccess.Enums;

namespace HomeServiceProvider.Dtos.Admin;

public class ResolveDisputeDto
{
    // Who the admin ruled in favour of
    [Required]
    public string Resolution { get; set; } = string.Empty; // "FavorCustomer" | "FavorProvider" | "Mutual"

    [Required]
    public string AdminNotes { get; set; } = string.Empty;

    // Admin sets the final booking status explicitly
    [Required]
    public BookingStatus FinalStatus { get; set; }  // Completed or Cancelled
}