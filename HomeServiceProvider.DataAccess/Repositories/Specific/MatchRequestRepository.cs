using HomeServiceProvider.DataAccess.Data;
using HomeServiceProvider.DataAccess.Entities;
using HomeServiceProvider.DataAccess.Repositories.Generics;
using Microsoft.EntityFrameworkCore;

namespace HomeServiceProvider.DataAccess.Repositories.Specific
{
    public class MatchRequestRepository : Repository<MatchRequest>, IMatchRequestRepository
    {
        public MatchRequestRepository(AppDbContext context) : base(context) { }

        public async Task<MatchRequest?> GetWithResultsAsync(Guid matchRequestId)
            => await _dbSet
                .Include(m => m.MatchResults.OrderBy(r => r.Rank))
                    .ThenInclude(r => r.ProviderProfile).ThenInclude(p => p.User)
                .Include(m => m.AIEvaluationLog)
                .FirstOrDefaultAsync(m => m.Id == matchRequestId);
    }

}
