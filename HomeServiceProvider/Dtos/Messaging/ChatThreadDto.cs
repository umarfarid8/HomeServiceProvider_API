namespace HomeServiceProvider.Dtos.Messaging;

// Full detail — used when a user opens a specific chat thread
public class ChatThreadDto
{
    public Guid ThreadId { get; set; }
    public Guid BookingId { get; set; }
    public string BookingStatus { get; set; } = string.Empty;
    public string OtherPartyName { get; set; } = string.Empty;
    public string ServiceCategory { get; set; } = string.Empty;
    public string ProblemDescription { get; set; } = string.Empty;  // context for the conversation
    public string ScheduledDate { get; set; } = string.Empty;
    public List<MessageDto> Messages { get; set; } = new();
}