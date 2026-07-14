namespace HomeServiceProvider.Dtos.Admin;

public class AdminUserDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsEmailVerified { get; set; }
    public DateTime JoinedAt { get; set; }

    // Provider-only fields (null for customers)
    public string? BusinessName { get; set; }
    public string? City { get; set; }
    public string? VerificationStatus { get; set; }
    public decimal? AverageRating { get; set; }
    public int? TotalJobsCompleted { get; set; }
}