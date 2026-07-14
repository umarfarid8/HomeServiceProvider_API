using HomeServiceProvider.DataAccess.Common;

namespace HomeServiceProvider.DataAccess.Entities
{
    public class Message : BaseEntity
    {
        public Guid ChatThreadId { get; set; }
        public Guid SenderId { get; set; }
        public string Content { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; }

        // Navigation
        public ChatThread ChatThread { get; set; } = null!;
        public User Sender { get; set; } = null!;
    }
}
