using HomeServiceProvider.DataAccess.Data;
using HomeServiceProvider.DataAccess.Entities;
using HomeServiceProvider.DataAccess.Repositories.Generics;
using Microsoft.EntityFrameworkCore;

namespace HomeServiceProvider.DataAccess.Repositories.Specific
{
    public class ReviewRepository : Repository<Review>, IReviewRepository
    {
        public ReviewRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Review>> GetByRevieweeIdAsync(Guid revieweeId)
            => await _dbSet
                .Where(r => r.RevieweeId == revieweeId && !r.IsFlagged)
                .Include(r => r.Reviewer)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

        public async Task<IEnumerable<Review>> GetRecentReviewsForProviderAsync(
            Guid revieweeId, int count = 20)
            => await _dbSet
                .Where(r => r.RevieweeId == revieweeId && !r.IsFlagged)
                .OrderByDescending(r => r.CreatedAt)
                .Take(count)
                .ToListAsync();

        public async Task<decimal> GetAverageRatingAsync(Guid revieweeId)
            => await _dbSet
                .Where(r => r.RevieweeId == revieweeId && !r.IsFlagged)
                .AverageAsync(r => (decimal?)r.Rating) ?? 0m;

        public async Task<IEnumerable<Review>> GetFlaggedReviewsAsync()
            => await _dbSet
                .Where(r => r.IsFlagged)
                .Include(r => r.Reviewer)
                .Include(r => r.Reviewee)
                .Include(r => r.Booking)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

        // ADD this method inside the existing ReviewRepository class
        public async Task<IEnumerable<Review>> GetByBookingIdAsync(Guid bookingId)
            => await _dbSet
                .Where(r => r.BookingId == bookingId)
                .Include(r => r.Reviewer)
                .Include(r => r.Reviewee)
                .ToListAsync();

    }
}
