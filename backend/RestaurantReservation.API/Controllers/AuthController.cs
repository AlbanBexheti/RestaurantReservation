using Microsoft.AspNetCore.Mvc;
using RestaurantReservation.API.DTOs.Auth;
using RestaurantReservation.API.Services.Interfaces;

namespace RestaurantReservation.API.Controllers
{
    // [ApiController] enables automatic model validation
    // If [Required] fields are missing it returns 400 automatically
    [ApiController]

    // [Route] defines the base URL: /api/auth
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // POST /api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var result = await _authService.RegisterAsync(dto);

            // null means email already taken
            if (result == null)
                return Conflict("Email is already registered.");

            // 201 Created with the token and user info
            return CreatedAtAction(nameof(Register), result);
        }

        // POST /api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);

            // null means wrong email or password
            if (result == null)
                return Unauthorized("Invalid email or password.");

            // 200 OK with JWT token
            return Ok(result);
        }
    }
}