using HomeServiceProvider.DataAccess.Data;
using HomeServiceProvider.DataAccess.Entities;
using HomeServiceProvider.DataAccess.Enums;
using HomeServiceProvider.DataAccess.Repositories.Generics;
using Microsoft.EntityFrameworkCore;

namespace HomeServiceProvider.DataAccess.Repositories.Specific
{
    public class ProviderProfileRepository : Repository<ProviderProfile>, IProviderProfileRepository
    {
        public ProviderProfileRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<ProviderProfile>> GetProvidersByCityAsync(string city)
            => await _dbSet
                .Where(p => p.City == city && p.IsActive &&
                            p.VerificationStatus == VerificationStatus.Approved)
                .Include(p => p.User)
                .Include(p => p.Services).ThenInclude(s => s.ServiceCategory)
                .ToListAsync();

        public async Task<IEnumerable<ProviderProfile>> GetProvidersForAIMatchAsync(
            string city, Guid? serviceCategoryId)
        {
            var query = _dbSet
                .Where(p => p.City == city && p.IsActive &&
                            p.VerificationStatus == VerificationStatus.Approved);

            if (serviceCategoryId.HasValue)
                query = query.Where(p => p.Services
                    .Any(s => s.ServiceCategoryId == serviceCategoryId));

            return await query
                .Include(p => p.User)
                .Include(p => p.Services).ThenInclude(s => s.ServiceCategory)
                // Reviews loaded separately via IReviewRepository for better control
                .ToListAsync();
        }

        public async Task<ProviderProfile?> GetWithServicesAndReviewsAsync(Guid providerId)
            => await _dbSet
                .Include(p => p.User)
                .Include(p => p.Services).ThenInclude(s => s.ServiceCategory)
                .Include(p => p.PortfolioImages)
                .FirstOrDefaultAsync(p => p.Id == providerId);

        public async Task<ProviderProfile?> GetFullProfileAsync(Guid providerId)
            => await _dbSet
                .Include(p => p.User)
                .Include(p => p.Services).ThenInclude(s => s.ServiceCategory)
                .Include(p => p.PortfolioImages)
                .Include(p => p.VerificationDocuments)
                .Include(p => p.AvailabilitySlots)
                .FirstOrDefaultAsync(p => p.Id == providerId);

        public async Task UpdateAverageRatingAsync(Guid providerId)
        {
            var provider = await _dbSet.FindAsync(providerId);
            if (provider is null) return;

            var avgRating = await _context.Reviews
                .Where(r => r.RevieweeId == provider.UserId && !r.IsFlagged)
                .AverageAsync(r => (decimal?)r.Rating) ?? 0m;

            var jobCount = await _context.Bookings
                .CountAsync(b => b.ProviderProfileId == providerId &&
                                 b.Status == BookingStatus.Completed);

            provider.AverageRating = Math.Round(avgRating, 2);
            provider.TotalJobsCompleted = jobCount;
        }
        // ADD these two methods inside the existing ProviderProfileRepository class

        public async Task<ProviderProfile?> GetByUserIdAsync(Guid userId)
            => await _dbSet.FirstOrDefaultAsync(p => p.UserId == userId);

        public async Task<ProviderProfile?> GetFullProfileByUserIdAsync(Guid userId)
            => await _dbSet
                .Include(p => p.User)
                .Include(p => p.Services).ThenInclude(s => s.ServiceCategory)
                .Include(p => p.PortfolioImages)
                .Include(p => p.VerificationDocuments)
                .Include(p => p.AvailabilitySlots)
                .FirstOrDefaultAsync(p => p.UserId == userId);
    }
}
