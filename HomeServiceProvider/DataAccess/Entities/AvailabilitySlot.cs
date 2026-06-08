using HomeServiceProvider.DataAccess.Common;

namespace HomeServiceProvider.DataAccess.Entities
{
    public class AvailabilitySlot : BaseEntity
    {
        public Guid ProviderProfileId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

        // True = repeats weekly; False = one-time override (e.g. holiday block)
        public bool IsRecurring { get; set; } = true;
        public DateTime? SpecificDate { get; set; }
        public bool IsAvailable { get; set; } = true;

        // Navigation
        public ProviderProfile ProviderProfile { get; set; } = null!;
    }
}
