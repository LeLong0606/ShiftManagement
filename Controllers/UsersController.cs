using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ShiftManagement.DTOs;
using ShiftManagement.Services;
using System.Security.Claims;

namespace ShiftManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult> GetUsers(
            [FromQuery] string? search = "",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            var (cached, data) = await _userService.GetUsersAsync(search, page, pageSize);
            return Ok(new { Cached = cached, Data = data });
        }

        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var dto = await _userService.GetUserAsync(id);
            if (dto == null) return NotFound();
            return Ok(dto);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> PostUser([FromBody] UserCreateDto input)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var (success, error, userId, username) = await _userService.CreateUserAsync(input);

            if (!success)
                return Conflict(new { Message = error });

            return CreatedAtAction(nameof(GetUser),
                new { id = userId },
                new { userId, username });
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> PutUser(int id, [FromBody] UserUpdateDto input)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var (success, error) = await _userService.UpdateUserAsync(id, input, User);
            if (!success && error == "NotFound") return NotFound();
            if (!success && error == "Forbid") return Forbid();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var (success, error) = await _userService.DeleteUserAsync(id);
            if (!success && error == "NotFound") return NotFound();

            return NoContent();
        }

        [HttpPatch("{id:int}/changepassword")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var (success, error) = await _userService.ChangePasswordAsync(id, dto, User);
            if (!success && error == "NotFound") return NotFound();
            if (!success && error == "Forbid") return Forbid();
            if (!success) return BadRequest(new { Message = error });

            return NoContent();
        }

        [HttpPatch("{id:int}/lock")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleLock(int id)
        {
            var (success, status, error) = await _userService.ToggleLockAsync(id);
            if (!success && error == "NotFound") return NotFound();

            return Ok(new { UserID = id, Status = status });
        }

        [HttpPost("{id:int}/roles")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddRole(int id, [FromBody] int roleId)
        {
            var (success, error) = await _userService.AddRoleAsync(id, roleId);
            if (!success)
            {
                if (error == "Không tìm thấy người dùng." || error == "Không tìm thấy role.")
                    return NotFound(new { Message = error });
                if (error == "User đã có role này.")
                    return Conflict(new { Message = error });
            }

            return Ok(new { Message = "Đã thêm role cho user." });
        }

        [HttpDelete("{id:int}/roles/{roleId:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveRole(int id, int roleId)
        {
            var (success, error) = await _userService.RemoveRoleAsync(id, roleId);
            if (!success && error == "NotFound") return NotFound();

            return Ok(new { Message = "Đã xóa role của user." });
        }

        [HttpGet("{id:int}/roles")]
        [Authorize]
        public async Task<IActionResult> GetUserRoles(int id)
        {
            var roles = await _userService.GetUserRolesAsync(id);
            return Ok(roles);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var token = await _userService.LoginAsync(dto);
            if (token == null)
                return Unauthorized(new { Message = "Tên đăng nhập hoặc mật khẩu không đúng." });

            return Ok(new { Token = token });
        }
    }
}