using HomeServiceProvider.DataAccess.Entities;
using HomeServiceProvider.DataAccess.Repositories.Generics;

namespace HomeServiceProvider.DataAccess.Repositories.Specific
{
    public interface IBookingRepository : IRepository<Booking>
    {
        Task<IEnumerable<Booking>> GetByCustomerIdAsync(Guid customerProfileId);
        Task<IEnumerable<Booking>> GetByProviderIdAsync(Guid providerProfileId);
        Task<Booking?> GetWithFullDetailsAsync(Guid bookingId);

        // ADD this line to the existing IBookingRepository interface
        Task<IEnumerable<Booking>> GetDisputedBookingsAsync();

        // Core scheduling conflict check — used before confirming any booking
        Task<bool> HasConflictAsync(
            Guid providerProfileId,
            DateTime date,
            TimeOnly start,
            TimeOnly end,
            Guid? excludeBookingId = null);
    }
}
