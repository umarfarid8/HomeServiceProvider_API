using HomeServiceProvider.Dtos.Admin;

namespace HomeServiceProvider.Services.Interfaces;

public interface IAdminService
{
    // ── Dashboard ─────────────────────────────────────────────────────────────
    Task<AdminDashboardDto> GetDashboardAsync();

    // ── User Management ───────────────────────────────────────────────────────
    Task<List<AdminUserDto>> GetUsersAsync(string? role, bool? isActive, string? search);
    Task<AdminUserDto> GetUserDetailAsync(Guid userId);
    Task<AdminUserDto> ToggleUserStatusAsync(Guid targetUserId, Guid adminId);

    // ── Provider Verification ─────────────────────────────────────────────────
    Task<List<PendingVerificationDto>> GetPendingVerificationsAsync();
    Task<PendingVerificationDto> GetVerificationDetailAsync(Guid providerProfileId);
    Task ApproveVerificationAsync(Guid providerProfileId, Guid adminId);
    Task RejectVerificationAsync(Guid providerProfileId, Guid adminId, VerificationDecisionDto dto);

    // ── Dispute Resolution ────────────────────────────────────────────────────
    Task<List<DisputedBookingDto>> GetDisputesAsync();
    Task<DisputedBookingDto> GetDisputeDetailAsync(Guid bookingId);
    Task ResolveDisputeAsync(Guid bookingId, Guid adminId, ResolveDisputeDto dto);

    // ── Analytics ─────────────────────────────────────────────────────────────
    Task<FinancialAnalyticsDto> GetAnalyticsAsync();

    // ── System Logs ───────────────────────────────────────────────────────────
    Task<List<SystemLogDto>> GetLogsAsync(DateTime? from, DateTime? to, string? action);

    Task<bool> CreateServiceCategoryAsync(CreateServiceCategoryDto dto);
    Task<List<SearchAnalyticsLogDto>> GetFailedSearchesAsync();

    // Add to IAdminService:

    // Categories
    Task<List<ServiceCategoryDto>> GetServiceCategoriesAsync();
    Task<ServiceCategoryDto> CreateServiceCategoryAsync(UpsertServiceCategoryDto dto, Guid adminId);
    Task<ServiceCategoryDto> UpdateServiceCategoryAsync(Guid id, UpsertServiceCategoryDto dto, Guid adminId);
    Task ToggleCategoryStatusAsync(Guid id, Guid adminId);

    // Prompt Templates
    Task<List<PromptTemplateDto>> GetPromptTemplatesAsync();
    Task<PromptTemplateDto> UpdatePromptTemplateAsync(Guid id, UpdatePromptTemplateDto dto, Guid adminId);
}