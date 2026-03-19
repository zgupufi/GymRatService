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
            return Ok(workout);
        }
    }
}
