using HomeServiceProvider.Dtos.Moderation;
using HomeServiceProvider.Extensions;
using HomeServiceProvider.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeServiceProvider.Controllers;

[ApiController]
[Route("api/admin/moderation")]
[Authorize(Roles = "Admin")]    // All moderation endpoints are admin-only
public class ModerationController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ModerationController(IReviewService reviewService)
        => _reviewService = reviewService;

    // GET api/admin/moderation/queue
    // Returns all reviews pending admin decision
    [HttpGet("queue")]
    public async Task<IActionResult> GetQueue()
    {
        var queue = await _reviewService.GetModerationQueueAsync();
        return Ok(queue);
    }

    // POST api/admin/moderation/{itemId}/decide
    // Admin approves (review goes public) or rejects (stays hidden)
    [HttpPost("{itemId:guid}/decide")]
    public async Task<IActionResult> Moderate(
        Guid itemId, [FromBody] ModerateReviewDto dto)
    {
        var adminId = User.GetUserId();
        var result = await _reviewService.ModerateReviewAsync(itemId, adminId, dto);
        return Ok(result);
    }
}