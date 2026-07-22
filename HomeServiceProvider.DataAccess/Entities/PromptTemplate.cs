using HomeServiceProvider.DataAccess.Common;

namespace HomeServiceProvider.DataAccess.Entities;

public class PromptTemplate : BaseEntity
{
    // Unique identifier used in code to look it up — e.g. "intent_classifier"
    public string TemplateKey { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // The full prompt text — placeholders use {{VARIABLE}} syntax
    public string Content { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    // Which placeholders this template uses — stored as comma-separated string
    // e.g. "CATEGORIES,QUERY" — informational only, not enforced
    public string? PlaceholderKeys { get; set; }
}