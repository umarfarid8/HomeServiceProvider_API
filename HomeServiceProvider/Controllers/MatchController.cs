using HomeServiceProvider.Dtos.Matching;
using HomeServiceProvider.Extensions;
using HomeServiceProvider.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeServiceProvider.Controllers;

[ApiController]
[Route("api/match")]
//[Authorize(Roles = "Customer")]
public class MatchController : ControllerBase
{
    private readonly IMatchingService _matchingService;
    private readonly IHybridMatchingService _hybridMatchingService;

    public MatchController(
        IMatchingService matchingService,
        IHybridMatchingService hybridMatchingService)
    {
        _matchingService = matchingService;
        _hybridMatchingService = hybridMatchingService;
    }

    // ── Old endpoint (Phase 4 — keep for now, can remove later) ──────────────
    // POST api/match
    [HttpPost]
    public async Task<IActionResult> FindProviders([FromBody] SubmitMatchRequestDto dto)
    {
        var userId = User.GetUserId();
        var result = await _matchingService.FindBestProvidersAsync(userId, dto);
        return Ok(result);
    }

    // ── New hybrid endpoint ───────────────────────────────────────────────────
    // POST api/match/hybrid
    [HttpPost("hybrid")]
    public async Task<IActionResult> HybridSearch([FromBody] HybridSearchRequestDto dto)
    {
        var userId = User.GetUserId();
        var result = await _hybridMatchingService.SearchAsync(userId, dto);
        return Ok(result);
    }


    // POST api/match/manual
    // No OpenAI call — pure DB filter by category + city sorting
    [HttpPost("manual")]
    public async Task<IActionResult> ManualSearch([FromBody] ManualSearchRequestDto dto)
    {
        var userId = User.GetUserId();
        var result = await _hybridMatchingService.ManualSearchAsync(userId, dto);
        return Ok(result);
    }
}