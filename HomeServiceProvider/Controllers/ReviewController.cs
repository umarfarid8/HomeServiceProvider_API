using HomeServiceProvider.Dtos.Review;
using HomeServiceProvider.Extensions;
using HomeServiceProvider.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeServiceProvider.Controllers;

[ApiController]
[Route("api/reviews")]
public class ReviewController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewController(IReviewService reviewService)
        => _reviewService = reviewService;

    // POST api/reviews
    // Customer or Provider submits a review for a completed booking
    [HttpPost]
    [Authorize(Roles = "Customer,Provider")]
    public async Task<IActionResult> SubmitReview([FromBody] SubmitReviewDto dto)
    {
        var userId = User.GetUserId();
        var review = await _reviewService.SubmitReviewAsync(userId, dto);
        return Ok(review);
    }

    // GET api/reviews/provider/{providerProfileId}
    // Public — anyone can see a provider's reviews (used on provider profile page)
    [HttpGet("provider/{providerProfileId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProviderReviews(Guid providerProfileId)
    {
        var summary = await _reviewService.GetProviderReviewsAsync(providerProfileId);
        return Ok(summary);
    }

    // GET api/reviews/booking/{bookingId}
    // Customer or Provider sees both reviews on their booking
    [HttpGet("booking/{bookingId:guid}")]
    [Authorize(Roles = "Customer,Provider")]
    public async Task<IActionResult> GetBookingReviews(Guid bookingId)
    {
        var userId = User.GetUserId();
        var reviews = await _reviewService.GetBookingReviewsAsync(bookingId, userId);
        return Ok(reviews);
    }
}