namespace HomeServiceProvider.Dtos.Admin;

public class DisputedBookingDto
{
    public Guid BookingId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public string ServiceCategory { get; set; } = string.Empty;
    public string ScheduledDate { get; set; } = string.Empty;
    public string ScheduledTime { get; set; } = string.Empty;
    public decimal EstimatedAmount { get; set; }
    public bool IsEmergency { get; set; }
    public DateTime DisputedAt { get; set; }

    // Full chat transcript so admin can read the conversation
    public List<TranscriptMessageDto> ChatTranscript { get; set; } = new();

    // Timeline of status changes
    public List<StatusChangeDto> StatusTimeline { get; set; } = new();
}

public class TranscriptMessageDto
{
    public string SenderName { get; set; } = string.Empty;
    public string SenderRole { get; set; } = string.Empty;   // "Customer" or "Provider"
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
}

public class StatusChangeDto
{
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime ChangedAt { get; set; }
}