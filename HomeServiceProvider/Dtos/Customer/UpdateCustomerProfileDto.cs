using System.ComponentModel.DataAnnotations;

namespace HomeServiceProvider.Dtos.Customer;

// All fields optional — only non-null fields are applied (PATCH-style update)
public class UpdateCustomerProfileDto
{
    [MaxLength(150)]
    public string? FullName { get; set; }

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(300)]
    public string? Address { get; set; }

    public string? ProfileImageUrl { get; set; }
}