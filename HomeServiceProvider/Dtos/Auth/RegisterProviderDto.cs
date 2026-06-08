using System.ComponentModel.DataAnnotations;

namespace HomeServiceProvider.Dtos.Auth;

public class RegisterProviderDto
{
    [Required, MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string BusinessName { get; set; } = string.Empty;

    [Required, MaxLength(1000)]
    public string Bio { get; set; } = string.Empty;

    [Required, MaxLength(15)]
    public string CNIC { get; set; } = string.Empty;   // Format: 12345-1234567-1

    [Required, MaxLength(100)]
    public string City { get; set; } = string.Empty;

    public double Latitude { get; set; }
    public double Longitude { get; set; }

    [Range(1, 100)]
    public int ServiceAreaRadiusKm { get; set; } = 10;

    [Range(0, 100000)]
    public decimal BaseHourlyRate { get; set; }
}