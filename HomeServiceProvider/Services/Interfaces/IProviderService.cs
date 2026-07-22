using HomeServiceProvider.Dtos.Provider;

namespace HomeServiceProvider.Services.Interfaces;

public interface IProviderService
{
    Task<ProviderProfileDto> GetProfileAsync(Guid userId);
    Task<ProviderProfileDto> UpdateProfileAsync(Guid userId, UpdateProviderProfileDto dto);
    Task<VerificationDocumentDto> AddVerificationDocumentAsync(Guid userId, AddVerificationDocumentDto dto);

    // Add to IProviderService:
    Task<List<ProviderServiceDto>> GetMyServicesAsync(Guid userId);
    Task<ProviderServiceDto> AddServiceAsync(Guid userId, AddProviderServiceDto dto);
    Task RemoveServiceAsync(Guid userId, Guid providerServiceId);
    Task<ProviderProfileDto> GetPublicProfileAsync(Guid providerProfileId);
}