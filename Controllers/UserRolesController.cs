using Microsoft.AspNetCore.Mvc;
using ShiftManagement.DTOs;
using ShiftManagement.Services;
using Microsoft.AspNetCore.Authorization;

namespace ShiftManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserRolesController : ControllerBase
    {
        private readonly UserRoleService _userRoleService;

        public UserRolesController(UserRoleService userRoleService)
        {
            _userRoleService = userRoleService;
        }

        /// <summary>
        /// [GET] Lấy danh sách user-role, có thể lọc theo userId/roleId.
        /// Chỉ Admin hoặc Manager mới được truy cập.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<IEnumerable<UserRoleDto>>> GetUserRoles(
            [FromQuery] int? userId = null,
            [FromQuery] int? roleId = null)
        {
            var result = await _userRoleService.GetUserRolesAsync(userId, roleId);
            return Ok(result);
        }

        /// <summary>
        /// [POST] Gán role cho user.
        /// Chỉ Admin mới có quyền gán.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserRoleDto>> PostUserRole([FromBody] UserRoleCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (resultDto, error) = await _userRoleService.AddUserRoleAsync(dto);

            if (error == "Không tìm thấy người dùng.")
                return NotFound(new { Message = error });
            if (error == "Không tìm thấy role.")
                return NotFound(new { Message = error });
            if (error == "User đã có role này.")
                return Conflict(new { Message = error });

            return CreatedAtAction(nameof(GetUserRole), new { userId = resultDto.UserID, roleId = resultDto.RoleID }, resultDto);
        }

        /// <summary>
        /// [GET] Xem thông tin user-role cụ thể.
        /// Chỉ Admin hoặc Manager mới được truy cập.
        /// </summary>
        [HttpGet("{userId:int}/{roleId:int}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<UserRoleDto>> GetUserRole(int userId, int roleId)
        {
            var dto = await _userRoleService.GetUserRoleAsync(userId, roleId);
            if (dto == null)
                return NotFound();
            return Ok(dto);
        }

        /// <summary>
        /// [DELETE] Xóa role của user.
        /// Chỉ Admin mới có quyền xóa.
        /// </summary>
        [HttpDelete("{userId:int}/{roleId:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUserRole(int userId, int roleId)
        {
            var deleted = await _userRoleService.DeleteUserRoleAsync(userId, roleId);
            if (!deleted)
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// [PUT] Đổi role của user (nâng cấp/sửa role).
        /// Chỉ Admin mới có quyền đổi.
        /// </summary>
        [HttpPut("{userId:int}/{roleId:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUserRole(int userId, int roleId, [FromBody] UserRoleUpdateDto dto)
        {
            var error = await _userRoleService.UpdateUserRoleAsync(userId, roleId, dto);

            if (error == "Không tìm thấy user-role.")
                return NotFound();
            if (error != null)
                return NotFound(new { Message = error });

            return NoContent();
        }
    }
}