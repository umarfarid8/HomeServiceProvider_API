namespace HomeServiceProvider.Dtos.Admin;

public class SearchAnalyticsLogDto
{
    public Guid Id { get; set; }
    public string RawQuery { get; set; } = string.Empty;
    public string? ClassifiedCategory { get; set; }
    public decimal ConfidenceScore { get; set; }
    public bool WasSuccessful { get; set; }
    public string? FailureReason { get; set; }
    public bool ServedFromCache { get; set; }
    public int ProvidersReturned { get; set; }
    public DateTime SearchedAt { get; set; }
}