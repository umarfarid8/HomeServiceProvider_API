namespace HomeServiceProvider.Dtos.Messaging;

public class MessageDto
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public bool IsMine { get; set; }   // true = show on right side (React), false = left side
    public bool IsRead { get; set; }
    public DateTime SentAt { get; set; }
}