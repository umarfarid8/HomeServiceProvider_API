using HomeServiceProvider.DataAccess.Common;
using HomeServiceProvider.DataAccess.Enums;

namespace HomeServiceProvider.DataAccess.Entities
{
    public class ModerationQueueItem : BaseEntity
    {
        public Guid ReviewId { get; set; }
        public string FlagReason { get; set; } = string.Empty;
        public ModerationStatus Status { get; set; } = ModerationStatus.Pending;
        public Guid? ReviewedByAdminId { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? AdminNotes { get; set; }

        // Navigation
        public Review Review { get; set; } = null!;
    }

}
