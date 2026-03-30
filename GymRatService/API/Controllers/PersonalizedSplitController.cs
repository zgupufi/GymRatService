using GymRatService.BLL.Core.Interfaces;
using GymRatService.BLL.Services;
using GymRatService.Common.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace GymRatService.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class PersonalizedSplitController : Controller
    {
        public readonly PersonalizedSplitsService _personalizedSplitService;

        public PersonalizedSplitController(IPersonalizedSplitsService personalizedSplitService)
        {
            _personalizedSplitService = (PersonalizedSplitsService?)personalizedSplitService;
        }
        [HttpGet]
        public async Task<IActionResult> GetPersonalizedSplit()
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var personalizedSplit = await _personalizedSplitService.GetUserWeeklySplitByUserIdAsync(userId);

            return Ok(personalizedSplit);
        }
        [HttpPut]
        public async Task<IActionResult> SavePersonalizedSplit([FromBody] UserWeeklySplit personalizedSplit)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            personalizedSplit.UserId = userId;
            var split = await _personalizedSplitService.SaveUserWeeklySplitAsync(personalizedSplit);

            return Ok();
        }
    }
}
