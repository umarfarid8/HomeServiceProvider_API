namespace HomeServiceProvider.Dtos.Admin;

public class SystemLogDto
{
    public Guid Id { get; set; }
    public string? PerformedBy { get; set; }       // Admin's name (or "System" if null)
    public string Action { get; set; } = string.Empty;
    public string TargetEntityType { get; set; } = string.Empty;
    public string? TargetEntityId { get; set; }
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; }
}