namespace HomeServiceProvider.Dtos.Messaging;

// Lightweight — used in the threads list (sidebar / inbox view)
public class ChatThreadSummaryDto
{
    public Guid ThreadId { get; set; }
    public Guid BookingId { get; set; }
    public string BookingStatus { get; set; } = string.Empty;
    public string OtherPartyName { get; set; } = string.Empty;
    public string ServiceCategory { get; set; } = string.Empty;
    public string ScheduledDate { get; set; } = string.Empty;
    public string? LastMessageContent { get; set; }    // null if no messages yet
    public DateTime? LastMessageAt { get; set; }
    public int UnreadCount { get; set; }               // badge number
}