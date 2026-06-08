using HomeServiceProvider.Dtos.Provider;

namespace HomeServiceProvider.Services.Interfaces;

public interface IProviderService
{
    Task<ProviderProfileDto> GetProfileAsync(Guid userId);
    Task<ProviderProfileDto> UpdateProfileAsync(Guid userId, UpdateProviderProfileDto dto);
    Task<VerificationDocumentDto> AddVerificationDocumentAsync(Guid userId, AddVerificationDocumentDto dto);
}