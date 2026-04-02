using GymRatService.BLL.Core.Interfaces;
using GymRatService.Common.DTOs.Workout;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GymRatService.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class WorkoutsController : Controller
    {
        private readonly IWorkoutsService _workoutService;
        public WorkoutsController(IWorkoutsService workoutService)
        {
            _workoutService = workoutService;
        }
        [HttpPost]
        public async Task<IActionResult> CreateWokout([FromBody] CreateWorkoutDTO createWorkoutDTO)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var workout = await _workoutService.CreateWorkoutAsync(userId, createWorkoutDTO);
            return Ok(new { id = workout.Id, name = workout.Name, dateCreated = workout.Date });
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateWorkout(Guid id, [FromBody] UpdateWorkoutDTO updateWorkoutDTO)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var workout = await _workoutService.UpdateWorkoutAsync(id, userId, updateWorkoutDTO);
            if (workout == null) return NotFound();
            return Ok(new { id = workout.Id, name = workout.Name, dateCreated = workout.Date });
        }
        [HttpGet]
        public async Task<IActionResult> GetWorkouts()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var workouts = await _workoutService.GetFormattedWorkoutsByUserIdAsync(userId);
            return Ok(workouts);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetWorkout(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var workoutDetails = await _workoutService.GetWorkoutDetailsAsync(id, userId);
            return Ok(workoutDetails);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWorkout(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _workoutService.DeleteWorkoutAsync(id, userId);
            if (!result) return NotFound();
            return NoContent();
        }

    }
}
