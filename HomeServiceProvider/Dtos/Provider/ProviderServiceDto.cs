namespace HomeServiceProvider.Dtos.Provider;

public class ProviderServiceDto
{
    public Guid Id { get; set; }
    public Guid ServiceCategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal HourlyRate { get; set; }
    public int YearsOfExperience { get; set; }
}

public class AddProviderServiceDto
{
    [System.ComponentModel.DataAnnotations.Required]
    public Guid ServiceCategoryId { get; set; }

    [System.ComponentModel.DataAnnotations.MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Range(100, 100000)]
    public decimal HourlyRate { get; set; }

    [System.ComponentModel.DataAnnotations.Range(0, 50)]
    public int YearsOfExperience { get; set; }
}