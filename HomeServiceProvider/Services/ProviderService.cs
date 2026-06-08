using HomeServiceProvider.DataAccess.Entities;
using HomeServiceProvider.Dtos.Provider;
using HomeServiceProvider.Services.Interfaces;
using HomeServiceProvider.UnitOfWork;

namespace HomeServiceProvider.Services;

public class ProviderService : IProviderService
{
    private readonly IUnitOfWork _uow;

    public ProviderService(IUnitOfWork uow) => _uow = uow;

    public async Task<ProviderProfileDto> GetProfileAsync(Guid userId)
    {
        // GetFullProfileByUserIdAsync (added in Phase 2) includes all navigations
        var profile = await _uow.ProviderProfiles.GetFullProfileByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("Provider profile not found.");

        return MapToDto(profile);
    }

    public async Task<ProviderProfileDto> UpdateProfileAsync(Guid userId, UpdateProviderProfileDto dto)
    {
        var profile = await _uow.ProviderProfiles.GetByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("Provider profile not found.");

        var user = await _uow.Users.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        // Patch-style update
        if (dto.BusinessName is not null) profile.BusinessName = dto.BusinessName.Trim();
        if (dto.Bio is not null) profile.Bio = dto.Bio.Trim();
        if (dto.PhoneNumber is not null) user.PhoneNumber = dto.PhoneNumber.Trim();
        if (dto.City is not null) profile.City = dto.City.Trim();
        if (dto.Latitude.HasValue) profile.Latitude = dto.Latitude.Value;
        if (dto.Longitude.HasValue) profile.Longitude = dto.Longitude.Value;
        if (dto.ServiceAreaRadiusKm.HasValue) profile.ServiceAreaRadiusKm = dto.ServiceAreaRadiusKm.Value;
        if (dto.BaseHourlyRate.HasValue) profile.BaseHourlyRate = dto.BaseHourlyRate.Value;
        if (dto.ProfileImageUrl is not null) profile.ProfileImageUrl = dto.ProfileImageUrl;

        _uow.ProviderProfiles.Update(profile);
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync();

        return await GetProfileAsync(userId);
    }

    public async Task<VerificationDocumentDto> AddVerificationDocumentAsync(
        Guid userId, AddVerificationDocumentDto dto)
    {
        var profile = await _uow.ProviderProfiles.GetByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("Provider profile not found.");

        // Check if this document type was already uploaded and approved
        var existing = await _uow.VerificationDocuments.FirstOrDefaultAsync(d =>
            d.ProviderProfileId == profile.Id &&
            d.DocumentType == dto.DocumentType &&
            d.Status == DataAccess.Enums.VerificationStatus.Approved);

        if (existing is not null)
            throw new InvalidOperationException(
                $"An approved {dto.DocumentType} document already exists.");

        var document = new VerificationDocument
        {
            ProviderProfileId = profile.Id,
            DocumentType = dto.DocumentType,
            DocumentUrl = dto.DocumentUrl,
            Status = DataAccess.Enums.VerificationStatus.Pending
        };

        await _uow.VerificationDocuments.AddAsync(document);
        await _uow.SaveChangesAsync();

        return new VerificationDocumentDto
        {
            Id = document.Id,
            DocumentType = document.DocumentType.ToString(),
            DocumentUrl = document.DocumentUrl,
            Status = document.Status.ToString(),
            UploadedAt = document.CreatedAt
        };
    }

    // ─── Private Helper ───────────────────────────────────────────────────────

    private static ProviderProfileDto MapToDto(ProviderProfile profile)
        => new()
        {
            ProfileId = profile.Id,
            UserId = profile.UserId,
            FullName = profile.User.FullName,
            Email = profile.User.Email,
            PhoneNumber = profile.User.PhoneNumber,
            BusinessName = profile.BusinessName,
            Bio = profile.Bio,
            City = profile.City,
            Latitude = profile.Latitude,
            Longitude = profile.Longitude,
            ServiceAreaRadiusKm = profile.ServiceAreaRadiusKm,
            BaseHourlyRate = profile.BaseHourlyRate,
            AverageRating = profile.AverageRating,
            TotalJobsCompleted = profile.TotalJobsCompleted,
            VerificationStatus = profile.VerificationStatus.ToString(),
            IsVerified = profile.IsVerified,
            IsEmailVerified = profile.User.IsEmailVerified,
            ProfileImageUrl = profile.ProfileImageUrl,
            MemberSince = profile.CreatedAt,
            VerificationDocuments = profile.VerificationDocuments.Select(d => new VerificationDocumentDto
            {
                Id = d.Id,
                DocumentType = d.DocumentType.ToString(),
                DocumentUrl = d.DocumentUrl,
                Status = d.Status.ToString(),
                AdminNotes = d.AdminNotes,
                UploadedAt = d.CreatedAt
            }).ToList()
        };
}