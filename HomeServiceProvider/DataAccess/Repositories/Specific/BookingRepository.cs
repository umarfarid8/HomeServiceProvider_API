using HomeServiceProvider.DataAccess.Data;
using HomeServiceProvider.DataAccess.Entities;
using HomeServiceProvider.DataAccess.Enums;
using HomeServiceProvider.DataAccess.Repositories.Generics;
using Microsoft.EntityFrameworkCore;

namespace HomeServiceProvider.DataAccess.Repositories.Specific
{
    public class BookingRepository : Repository<Booking>, IBookingRepository
    {
        public BookingRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Booking>> GetByCustomerIdAsync(Guid customerProfileId)
            => await _dbSet
                .Where(b => b.CustomerProfileId == customerProfileId)
                .Include(b => b.ProviderProfile).ThenInclude(p => p.User)
                .Include(b => b.ServiceCategory)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

        public async Task<IEnumerable<Booking>> GetByProviderIdAsync(Guid providerProfileId)
            => await _dbSet
                .Where(b => b.ProviderProfileId == providerProfileId)
                .Include(b => b.CustomerProfile).ThenInclude(c => c.User)
                .Include(b => b.ServiceCategory)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

        public async Task<bool> HasConflictAsync(
            Guid providerProfileId,
            DateTime date,
            TimeOnly start,
            TimeOnly end,
            Guid? excludeBookingId = null)
        {
            // A conflict exists when any active booking on the same date
            // overlaps the requested time window (start < existingEnd AND end > existingStart)
            var query = _dbSet.Where(b =>
                b.ProviderProfileId == providerProfileId &&
                b.ScheduledDate.Date == date.Date &&
                b.Status != BookingStatus.Cancelled &&
                b.ScheduledStartTime < end &&
                b.ScheduledEndTime > start);

            if (excludeBookingId.HasValue)
                query = query.Where(b => b.Id != excludeBookingId.Value);

            return await query.AnyAsync();
        }

        public async Task<Booking?> GetWithFullDetailsAsync(Guid bookingId)
            => await _dbSet
                .Include(b => b.CustomerProfile).ThenInclude(c => c.User)
                .Include(b => b.ProviderProfile).ThenInclude(p => p.User)
                .Include(b => b.ServiceCategory)
                .Include(b => b.StatusHistory)
                .Include(b => b.ChatThread).ThenInclude(ct => ct.Messages.OrderBy(m => m.CreatedAt))
                .Include(b => b.Invoice)
                .Include(b => b.Reviews)
                .FirstOrDefaultAsync(b => b.Id == bookingId);
    }

}
