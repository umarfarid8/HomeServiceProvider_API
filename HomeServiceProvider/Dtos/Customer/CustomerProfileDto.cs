namespace HomeServiceProvider.Dtos.Customer;

public class CustomerProfileDto
{
    public Guid ProfileId { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
    public bool IsEmailVerified { get; set; }
    public DateTime MemberSince { get; set; }
}