namespace HomeServiceProvider.Dtos.Admin;

public class PendingVerificationDto
{
    public Guid ProviderProfileId { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string CNIC { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public decimal BaseHourlyRate { get; set; }
    public string VerificationStatus { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; }
    public List<AdminDocumentDto> Documents { get; set; } = new();
}

public class AdminDocumentDto
{
    public Guid Id { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentUrl { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}