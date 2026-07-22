using HomeServiceProvider.Dtos.Admin;
using HomeServiceProvider.Extensions;
using HomeServiceProvider.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeServiceProvider.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
        => _adminService = adminService;

    // ── Service Categories ────────────────────────────────────────────────────────

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
        => Ok(await _adminService.GetServiceCategoriesAsync());

    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory([FromBody] UpsertServiceCategoryDto dto)
    {
        var adminId = User.GetUserId();
        var result = await _adminService.CreateServiceCategoryAsync(dto, adminId);
        return Ok(result);
    }

    [HttpPut("categories/{id:guid}")]
    public async Task<IActionResult> UpdateCategory(
        Guid id, [FromBody] UpsertServiceCategoryDto dto)
    {
        var adminId = User.GetUserId();
        var result = await _adminService.UpdateServiceCategoryAsync(id, dto, adminId);
        return Ok(result);
    }

    [HttpPatch("categories/{id:guid}/toggle")]
    public async Task<IActionResult> ToggleCategory(Guid id)
    {
        var adminId = User.GetUserId();
        await _adminService.ToggleCategoryStatusAsync(id, adminId);
        return Ok(new { message = "Category status updated." });
    }

    // ── Prompt Templates ──────────────────────────────────────────────────────────

    [HttpGet("prompt-templates")]
    public async Task<IActionResult> GetPromptTemplates()
        => Ok(await _adminService.GetPromptTemplatesAsync());

    [HttpPut("prompt-templates/{id:guid}")]
    public async Task<IActionResult> UpdatePromptTemplate(
        Guid id, [FromBody] UpdatePromptTemplateDto dto)
    {
        var adminId = User.GetUserId();
        var result = await _adminService.UpdatePromptTemplateAsync(id, dto, adminId);
        return Ok(result);
    }

    // ── Dashboard ─────────────────────────────────────────────────────────────────

    // GET api/admin/dashboard
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
        => Ok(await _adminService.GetDashboardAsync());

    // ── User Management ───────────────────────────────────────────────────────────

    // GET api/admin/users?role=Provider&isActive=true&search=ali
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] string? role,
        [FromQuery] bool? isActive,
        [FromQuery] string? search)
        => Ok(await _adminService.GetUsersAsync(role, isActive, search));

    // GET api/admin/users/{userId}
    [HttpGet("users/{userId:guid}")]
    public async Task<IActionResult> GetUserDetail(Guid userId)
        => Ok(await _adminService.GetUserDetailAsync(userId));

    // PATCH api/admin/users/{userId}/toggle-status
    [HttpPatch("users/{userId:guid}/toggle-status")]
    public async Task<IActionResult> ToggleUserStatus(Guid userId)
    {
        var adminId = User.GetUserId();
        var result = await _adminService.ToggleUserStatusAsync(userId, adminId);
        return Ok(result);
    }

    // ── Provider Verification ─────────────────────────────────────────────────────

    // GET api/admin/verifications/pending
    [HttpGet("verifications/pending")]
    public async Task<IActionResult> GetPendingVerifications()
        => Ok(await _adminService.GetPendingVerificationsAsync());

    // GET api/admin/verifications/{providerProfileId}
    [HttpGet("verifications/{providerProfileId:guid}")]
    public async Task<IActionResult> GetVerificationDetail(Guid providerProfileId)
        => Ok(await _adminService.GetVerificationDetailAsync(providerProfileId));

    // POST api/admin/verifications/{providerProfileId}/approve
    [HttpPost("verifications/{providerProfileId:guid}/approve")]
    public async Task<IActionResult> ApproveVerification(Guid providerProfileId)
    {
        var adminId = User.GetUserId();
        await _adminService.ApproveVerificationAsync(providerProfileId, adminId);
        return Ok(new { message = "Provider has been approved and can now accept bookings." });
    }

    // POST api/admin/verifications/{providerProfileId}/reject
    [HttpPost("verifications/{providerProfileId:guid}/reject")]
    public async Task<IActionResult> RejectVerification(
        Guid providerProfileId, [FromBody] VerificationDecisionDto dto)
    {
        var adminId = User.GetUserId();
        await _adminService.RejectVerificationAsync(providerProfileId, adminId, dto);
        return Ok(new { message = "Provider verification has been rejected." });
    }

    // ── Disputes ──────────────────────────────────────────────────────────────────

    // GET api/admin/disputes
    [HttpGet("disputes")]
    public async Task<IActionResult> GetDisputes()
        => Ok(await _adminService.GetDisputesAsync());

    // GET api/admin/disputes/{bookingId}
    [HttpGet("disputes/{bookingId:guid}")]
    public async Task<IActionResult> GetDisputeDetail(Guid bookingId)
        => Ok(await _adminService.GetDisputeDetailAsync(bookingId));

    // POST api/admin/disputes/{bookingId}/resolve
    [HttpPost("disputes/{bookingId:guid}/resolve")]
    public async Task<IActionResult> ResolveDispute(
        Guid bookingId, [FromBody] ResolveDisputeDto dto)
    {
        var adminId = User.GetUserId();
        await _adminService.ResolveDisputeAsync(bookingId, adminId, dto);
        return Ok(new { message = "Dispute has been resolved." });
    }

    // ── Analytics & Search Logs ───────────────────────────────────────────────────

    // GET api/admin/analytics
    [HttpGet("analytics")]
    public async Task<IActionResult> GetAnalytics()
        => Ok(await _adminService.GetAnalyticsAsync());

    // GET api/admin/search-analytics
    // Shows failed searches — reveals what services users want that don't exist yet
    [HttpGet("search-analytics")]
    public async Task<IActionResult> GetFailedSearches()
        => Ok(await _adminService.GetFailedSearchesAsync());

    // ── System Logs ───────────────────────────────────────────────────────────────

    // GET api/admin/logs?from=2025-01-01&to=2025-12-31&action=PROVIDER
    [HttpGet("logs")]
    public async Task<IActionResult> GetLogs(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? action)
        => Ok(await _adminService.GetLogsAsync(from, to, action));
}