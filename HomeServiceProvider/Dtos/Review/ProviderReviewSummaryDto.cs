namespace HomeServiceProvider.Dtos.Review;

// Full review summary for the provider's public profile page
public class ProviderReviewSummaryDto
{
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public Dictionary<int, int> RatingBreakdown { get; set; } = new();  // { 5:10, 4:5, 3:1, ... }
    public List<ReviewDto> Reviews { get; set; } = new();
}