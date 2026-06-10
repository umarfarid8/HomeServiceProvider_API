namespace HomeServiceProvider.Dtos.Matching;

// A single ranked provider in the AI result
public class ProviderMatchDto
{
    public Guid ProviderProfileId { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public decimal AverageRating { get; set; }
    public int TotalJobsCompleted { get; set; }
    public decimal BaseHourlyRate { get; set; }
    public string? ProfileImageUrl { get; set; }
    public int Rank { get; set; }
    public decimal AIScore { get; set; }          // 0.0 – 1.0 confidence score
    public string ExplanationTag { get; set; } = string.Empty;  // e.g. "Top specialist for drain issues"
}