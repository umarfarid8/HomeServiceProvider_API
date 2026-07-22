namespace HomeServiceProvider.Dtos.Admin;

public class ServiceCategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? IconUrl { get; set; }
    public bool IsActive { get; set; }
    public int ProviderCount { get; set; }   // how many providers offer this category
}

public class UpsertServiceCategoryDto
{
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.MaxLength(300)]
    public string Description { get; set; } = string.Empty;

    public string? IconUrl { get; set; }
    public bool IsActive { get; set; } = true;
}