namespace HomeServiceProvider.Dtos.Admin;

public class PromptTemplateDto
{
    public Guid Id { get; set; }
    public string TemplateKey { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? PlaceholderKeys { get; set; }
    public bool IsActive { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class UpdatePromptTemplateDto
{
    [System.ComponentModel.DataAnnotations.Required]
    public string Content { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.MaxLength(300)]
    public string? Description { get; set; }
}