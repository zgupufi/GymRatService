using GymRatService.BLL.Core.Interfaces;
using GymRatService.Common.DTOs.CompletedSets;
using GymRatService.Common.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GymRatService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompletedSetsController : Controller
    {
        public readonly ICompletedSetsService _completedSetsService;

        public CompletedSetsController(ICompletedSetsService completedSetsService)
        {
            _completedSetsService = completedSetsService;
        }
        [HttpPost]
        public async Task<IActionResult> SaveCompletedSet([FromBody] SaveCompletedSetsDTO completedSetsDto)
        {
            var setsToSave = new List<CompletedSet>();
            foreach (var completedSet in completedSetsDto.Sets)
            {
                var newCompletedSet = new CompletedSet
                {
                    WorkoutId = completedSetsDto.WorkoutId,
                    ExerciseCardId = completedSetsDto.ExerciseCardId,
                    Reps = completedSet.Reps,
                    WeightKg = completedSet.WeightKg,
                    SetNumber = completedSet.SetNumber,
                    CompletedAt = DateTime.UtcNow,
                    UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                };
                setsToSave.Add(newCompletedSet);
            }
            await _completedSetsService.SaveCompletedSetsAsync(setsToSave);
            return Ok();
        }

        [HttpGet("recent")]
        public async Task<IActionResult> GetRecentCompletedSets([FromQuery] Guid workoutId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var recentSets = await _completedSetsService.GetRecentSetsByWorkoutIdAsync(workoutId, userId);
            return Ok(recentSets);
        }
    }
}
