using HomeServiceProvider.DataAccess.Common;

namespace HomeServiceProvider.DataAccess.Entities
{
    public class ProviderService : BaseEntity
    {
        public Guid ProviderProfileId { get; set; }
        public Guid ServiceCategoryId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal HourlyRate { get; set; }
        public int YearsOfExperience { get; set; }

        // Navigation
        public ProviderProfile ProviderProfile { get; set; } = null!;
        public ServiceCategory ServiceCategory { get; set; } = null!;
    }
}
