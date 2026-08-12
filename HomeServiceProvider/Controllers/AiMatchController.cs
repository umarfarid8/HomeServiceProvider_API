using HomeServiceProvider.Dtos.Matching;
using HomeServiceProvider.Extensions;
using HomeServiceProvider.Services;
using Microsoft.AspNetCore.Mvc;

namespace HomeServiceProvider.Controllers
{
    [ApiController]
    [Route("api/ai-match")]

    public class AiMatchController : Controller
    {
        private readonly AiMatchService _aiMatchService;
        
        public AiMatchController(AiMatchService aiMatchService)
        {
            _aiMatchService = aiMatchService;
        }

        //[HttpPost("match")]
        //public async Task<IActionResult> SearchMatch([FromBody] AiMatchDto dto)
        //{
        //    var userId = User.GetUserId();
        //    var result = await _aiMatchService.SearchMatchAsync(userId, dto);
        //    return Ok(result);
        //}
    }
}
