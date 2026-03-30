using GymRatService.BLL.Core.Interfaces;
using GymRatService.Common.DTOs.CompletedSets;
using GymRatService.Common.Models;
using GymRatService.DAL.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GymRatService.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class CompletedSetsController : Controller
    {
        public readonly ICompletedSetsService _completedSetsService;
        private readonly IWorkoutRepository _workoutRepository;

        public CompletedSetsController(ICompletedSetsService completedSetsService, IWorkoutRepository workoutRepository)
        {
            _completedSetsService = completedSetsService;
            _workoutRepository = workoutRepository;
        }
        [HttpPost]
        public async Task<IActionResult> SaveCompletedSet([FromBody] SaveCompletedSetsDTO completedSetsDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var workout = await _workoutRepository.GetWorkoutByIdAndUserIdAsync(completedSetsDto.WorkoutId, userId);
            if (workout == null) {
                return Forbid();
            }
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

        [HttpGet("history")]
        public async Task<IActionResult> GetExerciseHistory([FromQuery] int exerciseCardId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var exerciseHistory = await _completedSetsService.GetExerciseHistoryAsync(exerciseCardId, userId);
            return Ok(exerciseHistory);
        }
    }
}
