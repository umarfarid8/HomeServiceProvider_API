using HomeServiceProvider.Dtos.Matching;

namespace HomeServiceProvider.Services.Interfaces;

public interface IMatchingService
{
    Task<MatchResultDto> FindBestProvidersAsync(Guid customerUserId, SubmitMatchRequestDto dto);
}