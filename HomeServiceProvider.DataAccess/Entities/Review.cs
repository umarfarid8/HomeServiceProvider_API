using HomeServiceProvider.DataAccess.Common;

namespace HomeServiceProvider.DataAccess.Entities
{
    public class Review : BaseEntity
    {
        public Guid BookingId { get; set; }
        public Guid ReviewerId { get; set; }
        public Guid RevieweeId { get; set; }
        public int Rating { get; set; }         // 1–5
        public string Comment { get; set; } = string.Empty;
        public decimal AuthenticityScore { get; set; } = 1.0m;  // 0.0–1.0
        public bool IsFlagged { get; set; } = false;
        public string? FlagReason { get; set; }
        public bool IsVerifiedTransaction { get; set; } = true;

        // Navigation
        public Booking Booking { get; set; } = null!;
        public User Reviewer { get; set; } = null!;
        public User Reviewee { get; set; } = null!;
        public ModerationQueueItem? ModerationQueueItem { get; set; }
    }
}
