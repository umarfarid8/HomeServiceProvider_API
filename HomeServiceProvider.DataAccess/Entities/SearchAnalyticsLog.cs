namespace HomeServiceProvider.DataAccess.Entities;

// Immutable audit of every search query — especially failed ones
// Admin uses this to discover what services are missing from the platform
public class SearchAnalyticsLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid? CustomerProfileId { get; set; }
    public string RawQuery { get; set; } = string.Empty;

    // What the AI classified the query as
    public string? ClassifiedCategory { get; set; }
    public decimal ConfidenceScore { get; set; }

    // Was the query useful (confidence >= threshold)?
    public bool WasSuccessful { get; set; }

    // If WasSuccessful = false: why did it fail?
    public string? FailureReason { get; set; }

    // How many providers were returned
    public int ProvidersReturned { get; set; }

    // Was this result served from cache (no LLM call)?
    public bool ServedFromCache { get; set; }

    // Navigation
    public CustomerProfile? CustomerProfile { get; set; }
}