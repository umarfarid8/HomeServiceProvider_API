using HomeServiceProvider.DataAccess.Common;
using HomeServiceProvider.DataAccess.Enums;
using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace HomeServiceProvider.DataAccess.Entities
{
    public class Booking : BaseEntity
    {
        public Guid CustomerProfileId { get; set; }
        public Guid ProviderProfileId { get; set; }
        public Guid ServiceCategoryId { get; set; }
        public Guid? MatchRequestId { get; set; }
        public string ProblemDescription { get; set; } = string.Empty;
        public DateTime ScheduledDate { get; set; }
        public TimeOnly ScheduledStartTime { get; set; }
        public TimeOnly ScheduledEndTime { get; set; }
        public BookingStatus Status { get; set; } = BookingStatus.Pending;
        public bool IsEmergency { get; set; } = false;
        public bool IsOffHours { get; set; } = false;
        public decimal EstimatedAmount { get; set; }
        public decimal? FinalAmount { get; set; }
        public string? CancellationReason { get; set; }

        // Navigation
        public CustomerProfile CustomerProfile { get; set; } = null!;
        public ProviderProfile ProviderProfile { get; set; } = null!;
        public ServiceCategory ServiceCategory { get; set; } = null!;
        public MatchRequest? MatchRequest { get; set; }
        public ICollection<BookingStatusHistory> StatusHistory { get; set; } = new List<BookingStatusHistory>();
        public ChatThread? ChatThread { get; set; }
        public Invoice? Invoice { get; set; }
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
