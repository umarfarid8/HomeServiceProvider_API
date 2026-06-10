using System.ComponentModel.DataAnnotations;

namespace HomeServiceProvider.Dtos.Review;

public class SubmitReviewDto
{
    [Required]
    public Guid BookingId { get; set; }

    [Required, Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
    public int Rating { get; set; }

    // Minimum 10 chars to discourage meaningless one-word reviews
    [Required, MinLength(10, ErrorMessage = "Please write at least 10 characters."),
     MaxLength(500)]
    public string Comment { get; set; } = string.Empty;
}