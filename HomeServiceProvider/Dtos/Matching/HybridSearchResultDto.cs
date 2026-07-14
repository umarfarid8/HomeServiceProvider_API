namespace HomeServiceProvider.Dtos.Matching;

public class HybridSearchResultDto
{
    // The category the AI classified the query into
    public string ClassifiedCategory { get; set; } = string.Empty;
    public decimal ConfidenceScore { get; set; }

    // Top-rated provider — shown in premium banner
    public HybridProviderDto? AiSuggestedProvider { get; set; }

    // The rest — shown in secondary grid
    public List<HybridProviderDto> RemainingProviders { get; set; } = new();

    // Stats
    public int TotalProvidersFound { get; set; }
    public bool ServedFromCache { get; set; }
}

public class HybridProviderDto
{
    public Guid ProviderProfileId { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public decimal AverageRating { get; set; }
    public int TotalJobsCompleted { get; set; }
    public int ReviewCount { get; set; }
    public decimal BaseHourlyRate { get; set; }
    public string? ProfileImageUrl { get; set; }
    public List<string> ServiceNames { get; set; } = new();
    public int ExperienceYears { get; set; }        // max across their services
}

// Returned when confidence < threshold — tells frontend to show rephrase prompt
public class ClassificationFailureDto
{
    public string Reason { get; set; } = string.Empty;
    public decimal ConfidenceScore { get; set; }
    public string? AiReasoning { get; set; }
    public List<string> SuggestedCategories { get; set; } = new();
}