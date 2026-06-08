using HomeServiceProvider.DataAccess.Entities;
using HomeServiceProvider.DataAccess.Repositories.Generics;

namespace HomeServiceProvider.DataAccess.Repositories.Specific
{
    public interface IReviewRepository : IRepository<Review>
    {
        Task<IEnumerable<Review>> GetByRevieweeIdAsync(Guid revieweeId);

        // Returns the most recent N reviews — used by AI Agent for prompt building
        Task<IEnumerable<Review>> GetRecentReviewsForProviderAsync(Guid revieweeId, int count = 20);
        Task<decimal> GetAverageRatingAsync(Guid revieweeId);
        Task<IEnumerable<Review>> GetFlaggedReviewsAsync();
    }

}
