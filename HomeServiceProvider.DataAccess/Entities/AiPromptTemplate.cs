namespace HomeServiceProvider.DataAccess.Entities;

public class AiPromptTemplate
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty; // e.g., "intent_classifier"
    public string Content { get; set; } = string.Empty; // Holds the markdown prompt template
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}