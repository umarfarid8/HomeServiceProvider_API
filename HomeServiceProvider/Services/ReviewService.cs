using HomeServiceProvider.DataAccess.Entities;
using HomeServiceProvider.DataAccess.Enums;
using HomeServiceProvider.Dtos.Moderation;
using HomeServiceProvider.Dtos.Review;
using HomeServiceProvider.Services.Interfaces;
using HomeServiceProvider.UnitOfWork;

namespace HomeServiceProvider.Services;

public class ReviewService : IReviewService
{
    // A review scoring below this threshold is automatically flagged for admin review
    private const decimal FlagThreshold = 0.5m;

    private readonly IUnitOfWork _uow;

    public ReviewService(IUnitOfWork uow) => _uow = uow;

    // ─── Submit Review ────────────────────────────────────────────────────────

    public async Task<ReviewDto> SubmitReviewAsync(Guid reviewerUserId, SubmitReviewDto dto)
    {
        // Step 1: Load booking with full details
        var booking = await _uow.Bookings.GetWithFullDetailsAsync(dto.BookingId)
            ?? throw new KeyNotFoundException("Booking not found.");

        // Step 2: Booking must be Completed before anyone can review
        if (booking.Status != BookingStatus.Completed)
            throw new InvalidOperationException(
                "Reviews can only be submitted for completed bookings.");

        // Step 3: Reviewer must be a participant
        bool isCustomer = booking.CustomerProfile.UserId == reviewerUserId;
        bool isProvider = booking.ProviderProfile.UserId == reviewerUserId;

        if (!isCustomer && !isProvider)
            throw new UnauthorizedAccessException(
                "You were not part of this booking.");

        // Step 4: Determine who is being reviewed (the other party)
        Guid revieweeUserId = isCustomer
            ? booking.ProviderProfile.UserId
            : booking.CustomerProfile.UserId;

        // Step 5: Each direction is allowed only once per booking
        var existingReviews = await _uow.Reviews.GetByBookingIdAsync(dto.BookingId);
        bool alreadyReviewed = existingReviews.Any(r => r.ReviewerId == reviewerUserId);

        if (alreadyReviewed)
            throw new InvalidOperationException(
                "You have already submitted a review for this booking.");

        // Step 6: Calculate authenticity score using heuristics
        var (score, flagReason) = await CalculateAuthenticityScoreAsync(
            reviewerUserId, dto.Comment, revieweeUserId);

        bool isFlagged = score < FlagThreshold;

        // Step 7: Save the review
        var review = new Review
        {
            BookingId = dto.BookingId,
            ReviewerId = reviewerUserId,
            RevieweeId = revieweeUserId,
            Rating = dto.Rating,
            Comment = dto.Comment.Trim(),
            AuthenticityScore = score,
            IsFlagged = isFlagged,
            FlagReason = flagReason,
            IsVerifiedTransaction = true   // We verified the booking is Completed above
        };

        await _uow.Reviews.AddAsync(review);
        await _uow.SaveChangesAsync();

        // Step 8: If flagged → send to moderation queue (stays hidden until admin decides)
        if (isFlagged && flagReason is not null)
        {
            var queueItem = new ModerationQueueItem
            {
                ReviewId = review.Id,
                FlagReason = flagReason,
                Status = ModerationStatus.Pending
            };
            await _uow.ModerationQueueItems.AddAsync(queueItem);
            await _uow.SaveChangesAsync();
        }
        else
        {
            // Step 9: Clean review — update provider's average rating immediately
            // (only customer→provider direction affects the provider's public rating)
            if (isCustomer)
            {
                await _uow.ProviderProfiles
                    .UpdateAverageRatingAsync(booking.ProviderProfileId);
                await _uow.SaveChangesAsync();
            }
        }

        return new ReviewDto
        {
            Id = review.Id,
            BookingId = review.BookingId,
            ReviewerName = booking.CustomerProfile.UserId == reviewerUserId
                ? booking.CustomerProfile.User.FullName
                : booking.ProviderProfile.User.FullName,
            Rating = review.Rating,
            Comment = review.Comment,
            IsFlagged = review.IsFlagged,
            CreatedAt = review.CreatedAt
        };
    }

    // ─── Get Provider Reviews (Public) ────────────────────────────────────────

    public async Task<ProviderReviewSummaryDto> GetProviderReviewsAsync(Guid providerProfileId)
    {
        var provider = await _uow.ProviderProfiles.GetByIdAsync(providerProfileId)
            ?? throw new KeyNotFoundException("Provider not found.");

        // GetByRevieweeIdAsync already filters out flagged reviews (Phase 1 implementation)
        var reviews = (await _uow.Reviews.GetByRevieweeIdAsync(provider.UserId)).ToList();

        return new ProviderReviewSummaryDto
        {
            AverageRating = reviews.Any()
                ? Math.Round(reviews.Average(r => (decimal)r.Rating), 2)
                : 0m,
            TotalReviews = reviews.Count,
            RatingBreakdown = new Dictionary<int, int>
            {
                [5] = reviews.Count(r => r.Rating == 5),
                [4] = reviews.Count(r => r.Rating == 4),
                [3] = reviews.Count(r => r.Rating == 3),
                [2] = reviews.Count(r => r.Rating == 2),
                [1] = reviews.Count(r => r.Rating == 1)
            },
            Reviews = reviews
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReviewDto
                {
                    Id = r.Id,
                    BookingId = r.BookingId,
                    ReviewerName = r.Reviewer.FullName,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    IsFlagged = false,   // Never expose flagged state publicly
                    CreatedAt = r.CreatedAt
                }).ToList()
        };
    }

    // ─── Get Reviews for a Booking ────────────────────────────────────────────

    public async Task<List<ReviewDto>> GetBookingReviewsAsync(Guid bookingId, Guid userId)
    {
        var booking = await _uow.Bookings.GetWithFullDetailsAsync(bookingId)
            ?? throw new KeyNotFoundException("Booking not found.");

        bool isCustomer = booking.CustomerProfile.UserId == userId;
        bool isProvider = booking.ProviderProfile.UserId == userId;

        if (!isCustomer && !isProvider)
            throw new UnauthorizedAccessException("You were not part of this booking.");

        var reviews = await _uow.Reviews.GetByBookingIdAsync(bookingId);

        return reviews.Select(r => new ReviewDto
        {
            Id = r.Id,
            BookingId = r.BookingId,
            ReviewerName = r.Reviewer.FullName,
            Rating = r.Rating,
            Comment = r.Comment,
            IsFlagged = r.IsFlagged,
            CreatedAt = r.CreatedAt
        }).ToList();
    }

    // ─── Admin: Moderation Queue ──────────────────────────────────────────────

    public async Task<List<ModerationQueueItemDto>> GetModerationQueueAsync()
    {
        // Load only Pending items — admin hasn't actioned these yet
        var items = await _uow.ModerationQueueItems.FindAsync(
            i => i.Status == ModerationStatus.Pending);

        var result = new List<ModerationQueueItemDto>();

        foreach (var item in items)
        {
            var reviews = await _uow.Reviews.GetByBookingIdAsync(
                (await _uow.Reviews.GetByIdAsync(item.ReviewId))!.BookingId);

            var review = await _uow.Reviews.GetByIdAsync(item.ReviewId);
            if (review is null) continue;

            // Load reviewer and reviewee separately
            var reviewer = await _uow.Users.GetByIdAsync(review.ReviewerId);
            var reviewee = await _uow.Users.GetByIdAsync(review.RevieweeId);

            result.Add(new ModerationQueueItemDto
            {
                ModerationItemId = item.Id,
                ReviewId = review.Id,
                ReviewerName = reviewer?.FullName ?? "Unknown",
                RevieweeName = reviewee?.FullName ?? "Unknown",
                Rating = review.Rating,
                Comment = review.Comment,
                AuthenticityScore = review.AuthenticityScore,
                FlagReason = item.FlagReason,
                Status = item.Status.ToString(),
                FlaggedAt = item.CreatedAt,
                AdminNotes = item.AdminNotes
            });
        }

        return result.OrderByDescending(i => i.FlaggedAt).ToList();
    }

    // ─── Admin: Approve or Reject a Flagged Review ────────────────────────────

    public async Task<ModerationQueueItemDto> ModerateReviewAsync(
        Guid moderationItemId, Guid adminUserId, ModerateReviewDto dto)
    {
        if (dto.Decision == ModerationStatus.Pending)
            throw new InvalidOperationException(
                "Decision must be Approved or Rejected, not Pending.");

        var item = await _uow.ModerationQueueItems.GetByIdAsync(moderationItemId)
            ?? throw new KeyNotFoundException("Moderation item not found.");

        if (item.Status != ModerationStatus.Pending)
            throw new InvalidOperationException(
                "This review has already been moderated.");

        var review = await _uow.Reviews.GetByIdAsync(item.ReviewId)
            ?? throw new KeyNotFoundException("Review not found.");

        // Update the moderation queue item
        item.Status = dto.Decision;
        item.ReviewedByAdminId = adminUserId;
        item.ReviewedAt = DateTime.UtcNow;
        item.AdminNotes = dto.AdminNotes?.Trim();

        _uow.ModerationQueueItems.Update(item);

        if (dto.Decision == ModerationStatus.Approved)
        {
            // Admin says review is legitimate — make it visible
            review.IsFlagged = false;
            review.FlagReason = null;
            _uow.Reviews.Update(review);

            // Now that it's approved, update the provider's rating if it's a customer review
            var reviewer = await _uow.Users.GetByIdAsync(review.ReviewerId);
            var reviewee = await _uow.Users.GetByIdAsync(review.RevieweeId);

            if (reviewer?.Role == UserRole.Customer && reviewee?.Role == UserRole.Provider)
            {
                var providerProfile = await _uow.ProviderProfiles
                    .GetByUserIdAsync(review.RevieweeId);
                if (providerProfile is not null)
                {
                    await _uow.ProviderProfiles
                        .UpdateAverageRatingAsync(providerProfile.Id);
                }
            }
        }
        // If Rejected: review stays flagged (IsFlagged = true) → stays hidden from public
        // No rating update needed

        await _uow.SaveChangesAsync();

        return new ModerationQueueItemDto
        {
            ModerationItemId = item.Id,
            ReviewId = review.Id,
            Rating = review.Rating,
            Comment = review.Comment,
            AuthenticityScore = review.AuthenticityScore,
            FlagReason = item.FlagReason,
            Status = item.Status.ToString(),
            FlaggedAt = item.CreatedAt,
            AdminNotes = item.AdminNotes
        };
    }

    // ─── Private: Authenticity Scoring Engine ─────────────────────────────────

    private async Task<(decimal score, string? flagReason)> CalculateAuthenticityScoreAsync(
        Guid reviewerUserId, string comment, Guid revieweeUserId)
    {
        decimal score = 1.0m;
        var flagReasons = new List<string>();

        // ── Heuristic 1: Comment length ───────────────────────────────────────
        // Very short comments like "Good" or "OK thanks" add no real signal
        if (comment.Trim().Length < 20)
        {
            score -= 0.30m;
            flagReasons.Add("Comment is too short to be meaningful");
        }

        // ── Heuristic 2: Generic keyword ratio ────────────────────────────────
        // If 70%+ of the comment words are filler words, it signals a fake review
        var genericKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "good", "great", "nice", "ok", "okay", "fine", "best",
            "excellent", "amazing", "perfect", "awesome", "superb",
            "fantastic", "wonderful", "brilliant", "outstanding"
        };

        var words = comment.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        int genericCount = words.Count(w => genericKeywords.Contains(w.Trim('.', '!', ',')));

        if (words.Length > 0 && (double)genericCount / words.Length >= 0.70)
        {
            score -= 0.20m;
            flagReasons.Add("Comment consists mostly of generic filler words");
        }

        // ── Heuristic 3: New reviewer account ─────────────────────────────────
        // Fake review farms often create brand-new accounts to post reviews
        var reviewer = await _uow.Users.GetByIdAsync(reviewerUserId);
        if (reviewer is not null &&
            (DateTime.UtcNow - reviewer.CreatedAt).TotalDays < 7)
        {
            score -= 0.30m;
            flagReasons.Add("Reviewer account is less than 7 days old");
        }

        // ── Heuristic 4: Review burst detection ───────────────────────────────
        // A provider suddenly getting 5+ reviews in 24 hours is suspicious
        int recentCount = await _uow.Reviews.CountAsync(r =>
            r.RevieweeId == revieweeUserId &&
            r.CreatedAt >= DateTime.UtcNow.AddHours(-24));

        if (recentCount >= 5)
        {
            score -= 0.20m;
            flagReasons.Add($"Unusual burst: {recentCount} reviews in the last 24 hours");
        }

        // ── Heuristic 5: First-ever review (soft signal only) ─────────────────
        // First review from an account is slightly less trustworthy, but not enough to flag alone
        bool isFirstReview = !await _uow.Reviews.ExistsAsync(r => r.ReviewerId == reviewerUserId);
        if (isFirstReview)
        {
            score -= 0.10m;
            // Note: this alone doesn't add to flagReasons — it's a soft deduction
        }

        // Clamp score to valid range [0.0, 1.0]
        score = Math.Max(0m, Math.Min(1.0m, score));

        string? flagReason = flagReasons.Any()
            ? string.Join(" | ", flagReasons)
            : null;

        return (score, flagReason);
    }
}