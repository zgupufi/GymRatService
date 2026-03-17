using GymRatService.BLL.Core.Interfaces;
using GymRatService.Common.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymRatService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExercisesController : Controller
    {
        private readonly IExercisesService _exercisesService;
        public ExercisesController(IExercisesService exercisesService)
        {
            _exercisesService = exercisesService;
        }

        [HttpGet("exercises")]
        public async Task<IActionResult>GetExercises()
        {
            var exercises=await _exercisesService.GetExercisesAsync();

            if (exercises==null)
                return BadRequest();

            return Ok(exercises);
        }
    }
}
