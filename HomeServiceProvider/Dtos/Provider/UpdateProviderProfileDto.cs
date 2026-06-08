using System.ComponentModel.DataAnnotations;

namespace HomeServiceProvider.Dtos.Provider;

public class UpdateProviderProfileDto
{
    [MaxLength(150)]
    public string? BusinessName { get; set; }

    [MaxLength(1000)]
    public string? Bio { get; set; }

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    [Range(1, 100)]
    public int? ServiceAreaRadiusKm { get; set; }

    [Range(0, 100000)]
    public decimal? BaseHourlyRate { get; set; }

    public string? ProfileImageUrl { get; set; }
}