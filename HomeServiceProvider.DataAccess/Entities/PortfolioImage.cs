using HomeServiceProvider.DataAccess.Common;

namespace HomeServiceProvider.DataAccess.Entities
{
    public class PortfolioImage : BaseEntity
    {
        public Guid ProviderProfileId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? Caption { get; set; }

        // Navigation
        public ProviderProfile ProviderProfile { get; set; } = null!;
    }
}
