namespace HomeServiceProvider.Dtos.Matching;

// The full response returned to the customer
public class MatchResultDto
{
    public Guid MatchRequestId { get; set; }
    public string ProblemDescription { get; set; } = string.Empty;
    public int TotalProvidersEvaluated { get; set; }
    public List<ProviderMatchDto> RankedProviders { get; set; } = new();
}