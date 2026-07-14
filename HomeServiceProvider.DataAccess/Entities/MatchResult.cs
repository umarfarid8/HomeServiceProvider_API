using HomeServiceProvider.DataAccess.Common;

namespace HomeServiceProvider.DataAccess.Entities
{
    public class MatchResult : BaseEntity
    {
        public Guid MatchRequestId { get; set; }
        public Guid ProviderProfileId { get; set; }
        public int Rank { get; set; }
        public string ExplanationTag { get; set; } = string.Empty;
        public decimal AIScore { get; set; }

        // Navigation
        public MatchRequest MatchRequest { get; set; } = null!;
        public ProviderProfile ProviderProfile { get; set; } = null!;
    }
}
