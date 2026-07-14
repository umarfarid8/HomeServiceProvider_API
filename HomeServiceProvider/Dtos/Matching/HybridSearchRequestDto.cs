using System.ComponentModel.DataAnnotations;

namespace HomeServiceProvider.Dtos.Matching;

public class HybridSearchRequestDto
{
    [Required, MinLength(5), MaxLength(500)]
    public string Query { get; set; } = string.Empty;
}