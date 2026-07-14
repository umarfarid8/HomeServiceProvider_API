using HomeServiceProvider.DataAccess.Common;

namespace HomeServiceProvider.DataAccess.Entities
{
    public class MatchRequest : BaseEntity
    {
        public Guid CustomerProfileId { get; set; }
        public string ProblemDescription { get; set; } = string.Empty;
        public Guid? ServiceCategoryId { get; set; }
        public string? City { get; set; }

        // Navigation
        public CustomerProfile CustomerProfile { get; set; } = null!;
        public ServiceCategory? ServiceCategory { get; set; }
        public ICollection<MatchResult> MatchResults { get; set; } = new List<MatchResult>();
        public AIEvaluationLog? AIEvaluationLog { get; set; }
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
