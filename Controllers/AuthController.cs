using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftManagement.DTOs;
using ShiftManagement.Services;
using System.Security.Claims;

namespace ShiftManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, message, userId) = await _authService.RegisterAsync(dto);

            if (!success)
                return BadRequest(message);

            return Ok(new { Message = message, UserID = userId });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, message, token, user) = await _authService.LoginAsync(dto);

            if (!success)
                return Unauthorized(message);

            return Ok(new
            {
                Message = message,
                Token = token,
                User = user
            });
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<AuthUserDto>> GetMe()
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var user = await _authService.GetMeAsync(userId);

            if (user == null)
                return Unauthorized();

            return Ok(user);
        }
    }
}