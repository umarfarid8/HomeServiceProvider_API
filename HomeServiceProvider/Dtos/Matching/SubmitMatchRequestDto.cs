using System.ComponentModel.DataAnnotations;

namespace HomeServiceProvider.Dtos.Matching;

public class SubmitMatchRequestDto
{
    // What the customer types in plain English
    [Required, MinLength(10), MaxLength(1000)]
    public string ProblemDescription { get; set; } = string.Empty;

    // Optional — if customer already knows the category (e.g., Plumbing)
    public Guid? ServiceCategoryId { get; set; }
}