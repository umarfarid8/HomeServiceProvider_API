namespace HomeServiceProvider.Dtos.Review;

public class ReviewDto
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public string ReviewerName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public bool IsFlagged { get; set; }
    public DateTime CreatedAt { get; set; }
}