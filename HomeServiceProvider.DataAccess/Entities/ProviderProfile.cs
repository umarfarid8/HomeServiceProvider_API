using HomeServiceProvider.DataAccess.Common;
using HomeServiceProvider.DataAccess.Enums;

namespace HomeServiceProvider.DataAccess.Entities
{
    public class ProviderProfile : BaseEntity
    {
        public Guid UserId { get; set; }
        public string BusinessName { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string CNIC { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int ServiceAreaRadiusKm { get; set; } = 10;
        public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Pending;
        public bool IsActive { get; set; } = true;
        public decimal AverageRating { get; set; } = 0m;
        public int TotalJobsCompleted { get; set; } = 0;
        public decimal BaseHourlyRate { get; set; }
        public string? ProfileImageUrl { get; set; }

        // Computed — not mapped to DB column
        public bool IsVerified => VerificationStatus == VerificationStatus.Approved;

        // Navigation
        public User User { get; set; } = null!;
        public ICollection<VerificationDocument> VerificationDocuments { get; set; } = new List<VerificationDocument>();
        public ICollection<ProviderService> Services { get; set; } = new List<ProviderService>();
        public ICollection<PortfolioImage> PortfolioImages { get; set; } = new List<PortfolioImage>();
        public ICollection<AvailabilitySlot> AvailabilitySlots { get; set; } = new List<AvailabilitySlot>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<MatchResult> MatchResults { get; set; } = new List<MatchResult>();
    }
}
