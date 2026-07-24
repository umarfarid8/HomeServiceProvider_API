using HomeServiceProvider.DataAccess.Entities;
using HomeServiceProvider.Dtos.Provider;
using HomeServiceProvider.Services.Interfaces;
using HomeServiceProvider.UnitOfWork;

namespace HomeServiceProvider.Services;

public class ProviderService : IProviderService
{
    private readonly IUnitOfWork _uow;

    public ProviderService(IUnitOfWork uow) => _uow = uow;

    // ── Profile Management ───────────────────────────────────────────────────

    public async Task<ProviderProfileDto> GetProfileAsync(Guid userId)
    {
        var profile = await _uow.ProviderProfiles.GetFullProfileByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("Provider profile not found.");

        return MapToDto(profile);
    }

    public async Task<ProviderProfileDto> GetPublicProfileAsync(Guid providerProfileId)
    {
        var profile = await _uow.ProviderProfiles.GetFullProfileAsync(providerProfileId)
            ?? throw new KeyNotFoundException("Provider not found.");

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

    // ── Verification Documents ───────────────────────────────────────────────

    public async Task<VerificationDocumentDto> AddVerificationDocumentAsync(
        Guid userId, AddVerificationDocumentDto dto)
    {
        var profile = await _uow.ProviderProfiles.GetByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("Provider profile not found.");

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

    // ── Provider Availability (Using AvailabilitySlots) ─────────────────────

    public async Task<List<ProviderAvailabilityDto>> GetAvailabilityAsync(Guid userId)
    {
        var profile = await _uow.ProviderProfiles.GetByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("Provider profile not found.");

        var slots = await _uow.AvailabilitySlots.FindAsync(
            a => a.ProviderProfileId == profile.Id);

        return slots
            .OrderBy(a => a.DayOfWeek)
            .Select(a => new ProviderAvailabilityDto
            {
                Id = a.Id,
                DayOfWeek = a.DayOfWeek,
                StartTime = a.StartTime.ToTimeSpan(),
                EndTime = a.EndTime.ToTimeSpan(),
                IsAvailable = a.IsAvailable
            }).ToList();
    }

    public async Task SetAvailabilityAsync(Guid userId, List<SetAvailabilityDto> dtos)
    {
        var profile = await _uow.ProviderProfiles.GetByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("Provider profile not found.");

        // 1. Fetch existing slots
        var existingSlots = await _uow.AvailabilitySlots.FindAsync(
            a => a.ProviderProfileId == profile.Id);

        // 2. Remove old slots
        _uow.AvailabilitySlots.RemoveRange(existingSlots);

        // 3. Add new slots
        foreach (var dto in dtos)
        {
            if (dto.IsAvailable)
            {
                await _uow.AvailabilitySlots.AddAsync(new AvailabilitySlot
                {
                    ProviderProfileId = profile.Id,
                    DayOfWeek = dto.DayOfWeek,
                    StartTime = TimeOnly.FromTimeSpan(dto.StartTime),
                    EndTime = TimeOnly.FromTimeSpan(dto.EndTime),
                    IsAvailable = true,
                    IsRecurring = true
                });
            }
        }

        // 4. Save atomic changes safely without manual transaction conflict
        await _uow.SaveChangesAsync();
    }

    // ── Provider Services Management ─────────────────────────────────────────

    public async Task<List<ProviderServiceDto>> GetMyServicesAsync(Guid userId)
    {
        var profile = await _uow.ProviderProfiles.GetByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("Provider profile not found.");

        var services = await _uow.ProviderServices.FindAsync(
            ps => ps.ProviderProfileId == profile.Id);

        var result = new List<ProviderServiceDto>();
        foreach (var svc in services)
        {
            var cat = await _uow.ServiceCategories.GetByIdAsync(svc.ServiceCategoryId);
            result.Add(new ProviderServiceDto
            {
                Id = svc.Id,
                ServiceCategoryId = svc.ServiceCategoryId,
                CategoryName = cat?.Name ?? string.Empty,
                Description = svc.Description,
                HourlyRate = svc.HourlyRate,
                YearsOfExperience = svc.YearsOfExperience
            });
        }
        return result;
    }

    public async Task<ProviderServiceDto> AddServiceAsync(Guid userId, AddProviderServiceDto dto)
    {
        var profile = await _uow.ProviderProfiles.GetByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("Provider profile not found.");

        var category = await _uow.ServiceCategories.GetByIdAsync(dto.ServiceCategoryId)
            ?? throw new KeyNotFoundException("Service category not found.");

        if (!category.IsActive)
            throw new InvalidOperationException("This service category is no longer active.");

        bool alreadyAdded = await _uow.ProviderServices.ExistsAsync(
            ps => ps.ProviderProfileId == profile.Id &&
                  ps.ServiceCategoryId == dto.ServiceCategoryId);

        if (alreadyAdded)
            throw new InvalidOperationException(
                $"You have already added {category.Name} to your services.");

        var service = new DataAccess.Entities.ProviderService
        {
            ProviderProfileId = profile.Id,
            ServiceCategoryId = dto.ServiceCategoryId,
            Description = dto.Description.Trim(),
            HourlyRate = dto.HourlyRate,
            YearsOfExperience = dto.YearsOfExperience
        };

        await _uow.ProviderServices.AddAsync(service);
        await _uow.SaveChangesAsync();

        return new ProviderServiceDto
        {
            Id = service.Id,
            ServiceCategoryId = service.ServiceCategoryId,
            CategoryName = category.Name,
            Description = service.Description,
            HourlyRate = service.HourlyRate,
            YearsOfExperience = service.YearsOfExperience
        };
    }

    public async Task RemoveServiceAsync(Guid userId, Guid providerServiceId)
    {
        var profile = await _uow.ProviderProfiles.GetByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("Provider profile not found.");

        var service = await _uow.ProviderServices.GetByIdAsync(providerServiceId)
            ?? throw new KeyNotFoundException("Service not found.");

        if (service.ProviderProfileId != profile.Id)
            throw new UnauthorizedAccessException("You can only remove your own services.");

        _uow.ProviderServices.Remove(service);
        await _uow.SaveChangesAsync();
    }

    // ── Private Helper ───────────────────────────────────────────────────────

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