using HomeServiceProvider.Dtos.Matching;

namespace HomeServiceProvider.Services.Interfaces;

public interface IHybridMatchingService
{
    Task<HybridSearchResultDto> SearchAsync(Guid customerUserId, HybridSearchRequestDto dto);
    Task<HybridSearchResultDto> ManualSearchAsync(Guid customerUserId, ManualSearchRequestDto dto);
}