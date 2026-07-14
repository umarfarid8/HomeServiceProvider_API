using System.ComponentModel.DataAnnotations;

namespace HomeServiceProvider.Dtos.Admin;

public class CreateServiceCategoryDto
{
    [Required, MinLength(2), MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required, MinLength(5), MaxLength(500)]
    public string Description { get; set; } = string.Empty;
}