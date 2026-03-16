using GymRatService.BLL.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

using GymRatService.Common.DTOs;
using GymRatService.BLL.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using GymRatService.Common.Models;

namespace GymRatService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request is null)
            {
                return BadRequest();
            }

            var result = await _authService.LoginAsync(request);

            if (result is null)
            {
                return Unauthorized(new { Message = "Invalid credentials" });
            }

            return Ok(result);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest registerUser)
        {             if (registerUser is null)
            {
                return BadRequest();
            }
            try
            {
                var result = await _authService.RegisterAsync(registerUser);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { Message = ex.Message });
            }
        }
    }
}