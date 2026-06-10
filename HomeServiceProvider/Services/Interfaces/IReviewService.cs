using HomeServiceProvider.Dtos.Moderation;
using HomeServiceProvider.Dtos.Review;

namespace HomeServiceProvider.Services.Interfaces;

public interface IReviewService
{
    // Customer or Provider submits a review for a completed booking
    Task<ReviewDto> SubmitReviewAsync(Guid reviewerUserId, SubmitReviewDto dto);

    // Public: get all non-flagged reviews for a provider
    Task<ProviderReviewSummaryDto> GetProviderReviewsAsync(Guid providerProfileId);

    // Get both reviews on a specific booking (for booking detail page)
    Task<List<ReviewDto>> GetBookingReviewsAsync(Guid bookingId, Guid userId);

    // ── Admin Moderation ──────────────────────────────────────────────────────

    // Get all pending flagged reviews for the admin queue
    Task<List<ModerationQueueItemDto>> GetModerationQueueAsync();

    // Admin approves or rejects a flagged review
    Task<ModerationQueueItemDto> ModerateReviewAsync(
        Guid moderationItemId, Guid adminUserId, ModerateReviewDto dto);
}