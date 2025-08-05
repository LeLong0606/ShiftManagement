using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftManagement.Data;
using ShiftManagement.DTOs;
using ShiftManagement.Models;
using Microsoft.AspNetCore.Authorization;

namespace ShiftManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserRolesController : ControllerBase
    {
        private readonly ShiftManagementContext _context;

        public UserRolesController(ShiftManagementContext context)
        {
            _context = context;
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
            // Tối ưu hiệu suất: chỉ include khi cần thiết
            var query = _context.UserRoles.AsNoTracking().AsQueryable();

            if (userId.HasValue)
                query = query.Where(ur => ur.UserID == userId.Value);
            if (roleId.HasValue)
                query = query.Where(ur => ur.RoleID == roleId.Value);

            var result = await query
                .Include(ur => ur.User)
                .Include(ur => ur.Role)
                .Select(ur => new UserRoleDto
                {
                    UserID = ur.UserID,
                    Username = ur.User.Username,
                    RoleID = ur.RoleID,
                    RoleName = ur.Role.RoleName
                })
                .ToListAsync();

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

            // Kiểm tra tồn tại user & role
            if (!await _context.Users.AnyAsync(u => u.UserID == dto.UserID))
                return NotFound(new { Message = "Không tìm thấy người dùng." });

            if (!await _context.Roles.AnyAsync(r => r.RoleID == dto.RoleID))
                return NotFound(new { Message = "Không tìm thấy role." });

            // Kiểm tra trùng role
            if (await _context.UserRoles.AnyAsync(ur => ur.UserID == dto.UserID && ur.RoleID == dto.RoleID))
                return Conflict(new { Message = "User đã có role này." });

            var entity = new UserRole
            {
                UserID = dto.UserID,
                RoleID = dto.RoleID
            };

            _context.UserRoles.Add(entity);
            await _context.SaveChangesAsync();

            // Trả về DTO cho client
            var role = await _context.Roles.FindAsync(dto.RoleID);
            var user = await _context.Users.FindAsync(dto.UserID);
            var resultDto = new UserRoleDto
            {
                UserID = dto.UserID,
                Username = user?.Username ?? "",
                RoleID = dto.RoleID,
                RoleName = role?.RoleName ?? ""
            };

            return CreatedAtAction(nameof(GetUserRole), new { userId = dto.UserID, roleId = dto.RoleID }, resultDto);
        }

        /// <summary>
        /// [GET] Xem thông tin user-role cụ thể.
        /// Chỉ Admin hoặc Manager mới được truy cập.
        /// </summary>
        [HttpGet("{userId:int}/{roleId:int}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<UserRoleDto>> GetUserRole(int userId, int roleId)
        {
            var ur = await _context.UserRoles
                .Include(x => x.User)
                .Include(x => x.Role)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserID == userId && x.RoleID == roleId);

            if (ur == null)
                return NotFound();

            var dto = new UserRoleDto
            {
                UserID = ur.UserID,
                Username = ur.User.Username,
                RoleID = ur.RoleID,
                RoleName = ur.Role.RoleName
            };

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
            var ur = await _context.UserRoles
                .FirstOrDefaultAsync(x => x.UserID == userId && x.RoleID == roleId);

            if (ur == null)
                return NotFound();

            _context.UserRoles.Remove(ur);
            await _context.SaveChangesAsync();

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
            var ur = await _context.UserRoles
                .FirstOrDefaultAsync(x => x.UserID == userId && x.RoleID == roleId);

            if (ur == null)
                return NotFound();

            if (!await _context.Roles.AnyAsync(r => r.RoleID == dto.NewRoleID))
                return NotFound(new { Message = "Role mới không tồn tại." });

            ur.RoleID = dto.NewRoleID;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}