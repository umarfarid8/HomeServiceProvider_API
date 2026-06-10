namespace HomeServiceProvider.Dtos.Moderation;

public class ModerationQueueItemDto
{
    public Guid ModerationItemId { get; set; }
    public Guid ReviewId { get; set; }

    // Review content
    public string ReviewerName { get; set; } = string.Empty;
    public string RevieweeName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;

    // Why it was flagged
    public decimal AuthenticityScore { get; set; }
    public string FlagReason { get; set; } = string.Empty;

    // Current state
    public string Status { get; set; } = string.Empty;
    public DateTime FlaggedAt { get; set; }
    public string? AdminNotes { get; set; }
}