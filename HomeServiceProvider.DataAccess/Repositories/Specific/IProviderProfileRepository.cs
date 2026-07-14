using HomeServiceProvider.DataAccess.Entities;
using HomeServiceProvider.DataAccess.Repositories.Generics;

namespace HomeServiceProvider.DataAccess.Repositories.Specific
{
    public interface IProviderProfileRepository : IRepository<ProviderProfile>
    {
        Task<IEnumerable<ProviderProfile>> GetProvidersByCityAsync(string city);

        // Used by AI Agent — loads profile + services + recent reviews for prompt building
        Task<IEnumerable<ProviderProfile>> GetProvidersForAIMatchAsync(string city, Guid? serviceCategoryId);

        Task<ProviderProfile?> GetWithServicesAndReviewsAsync(Guid providerId);
        Task<ProviderProfile?> GetFullProfileAsync(Guid providerId);
        Task UpdateAverageRatingAsync(Guid providerId);

        // ADD these two methods to the existing interface — everything else stays the same
        Task<ProviderProfile?> GetByUserIdAsync(Guid userId);
        Task<ProviderProfile?> GetFullProfileByUserIdAsync(Guid userId);
    }

}
