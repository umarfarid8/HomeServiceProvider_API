using System.ComponentModel.DataAnnotations;

namespace HomeServiceProvider.Dtos.Matching;

public class ManualSearchRequestDto
{
    [Required]
    public Guid ServiceCategoryId { get; set; }

    // Optional — override customer's profile city
    [MaxLength(100)]
    public string? City { get; set; }
}