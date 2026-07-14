using HomeServiceProvider.DataAccess.Common;
using HomeServiceProvider.DataAccess.Enums;

namespace HomeServiceProvider.DataAccess.Entities
{
    public class BookingStatusHistory : BaseEntity
    {
        public Guid BookingId { get; set; }
        public BookingStatus Status { get; set; }
        public Guid ChangedByUserId { get; set; }
        public string? Notes { get; set; }

        // Navigation
        public Booking Booking { get; set; } = null!;
    }

}
