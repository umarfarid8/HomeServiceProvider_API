namespace HomeServiceProvider.DataAccess.Entities
{
    public class SystemLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid? PerformedByUserId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string TargetEntityType { get; set; } = string.Empty;
        public string? TargetEntityId { get; set; }
        public string? Details { get; set; }
        public string? IpAddress { get; set; }

        // Navigation
        public User? PerformedByUser { get; set; }
    }
}
