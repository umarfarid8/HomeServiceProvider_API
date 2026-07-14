using HomeServiceProvider.DataAccess.Entities;
using HomeServiceProvider.DataAccess.Repositories.Generics;

namespace HomeServiceProvider.DataAccess.Repositories.Specific
{
    public interface IMatchRequestRepository : IRepository<MatchRequest>
    {
        Task<MatchRequest?> GetWithResultsAsync(Guid matchRequestId);
    }
}
