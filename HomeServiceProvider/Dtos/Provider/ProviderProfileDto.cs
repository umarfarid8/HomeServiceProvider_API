namespace HomeServiceProvider.Dtos.Provider;

public class ProviderProfileDto
{
    public Guid ProfileId { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int ServiceAreaRadiusKm { get; set; }
    public decimal BaseHourlyRate { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalJobsCompleted { get; set; }
    public string VerificationStatus { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
    public bool IsEmailVerified { get; set; }
    public string? ProfileImageUrl { get; set; }
    public DateTime MemberSince { get; set; }
    public List<VerificationDocumentDto> VerificationDocuments { get; set; } = new();
}