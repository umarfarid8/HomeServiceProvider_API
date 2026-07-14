using HomeServiceProvider.Dtos.Matching;

namespace HomeServiceProvider.Services.Interfaces;

public interface IHybridMatchingService
{
    Task<HybridSearchResultDto> SearchAsync(Guid customerUserId, HybridSearchRequestDto dto);
}