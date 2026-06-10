using HomeServiceProvider.Dtos.Matching;
using HomeServiceProvider.Extensions;
using HomeServiceProvider.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeServiceProvider.Controllers;

[ApiController]
[Route("api/match")]
[Authorize(Roles = "Customer")]
public class MatchController : ControllerBase
{
    private readonly IMatchingService _matchingService;

    public MatchController(IMatchingService matchingService)
        => _matchingService = matchingService;

    // POST api/match
    // Customer submits a problem description and gets back ranked providers
    [HttpPost]
    public async Task<IActionResult> FindProviders([FromBody] SubmitMatchRequestDto dto)
    {
        var userId = User.GetUserId();
        var result = await _matchingService.FindBestProvidersAsync(userId, dto);
        return Ok(result);
    }
}