using HomeServiceProvider.DataAccess.Common;

namespace HomeServiceProvider.DataAccess.Entities
{
    public class CustomerProfile : BaseEntity
    {
        public Guid UserId { get; set; }
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string? ProfileImageUrl { get; set; }

        // Navigation
        public User User { get; set; } = null!;
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<MatchRequest> MatchRequests { get; set; } = new List<MatchRequest>();
    }
}
