using HomeServiceProvider.DataAccess.Common;

namespace HomeServiceProvider.DataAccess.Entities
{
    public class ChatThread : BaseEntity
    {
        public Guid BookingId { get; set; }

        // Navigation
        public Booking Booking { get; set; } = null!;
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
