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
    public class RolesController : ControllerBase
    {
        private readonly ShiftManagementContext _context;

        public RolesController(ShiftManagementContext context)
        {
            _context = context;
        }

        /// <summary>
        /// [GET] Lấy danh sách tất cả role. Chỉ tài khoản đã đăng nhập mới truy cập được.
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<RoleDto>>> GetRoles(
            [FromQuery] string? search = "",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 50;

            var query = _context.Roles.AsNoTracking();

            if (!string.IsNullOrEmpty(search))
            {
                string pattern = $"%{search}%";
                query = query.Where(r => EF.Functions.Like(r.RoleName, pattern));
            }

            var roles = await query
                .OrderBy(r => r.RoleID)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new RoleDto
                {
                    RoleID = r.RoleID,
                    RoleName = r.RoleName
                    // Nếu có thêm trường trong DTO thì bổ sung ở đây
                })
                .ToListAsync();

            return Ok(roles);
        }

        /// <summary>
        /// [GET] Lấy thông tin chi tiết một role theo ID. Chỉ tài khoản đã đăng nhập mới truy cập được.
        /// </summary>
        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<ActionResult<RoleDto>> GetRole(int id)
        {
            var role = await _context.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.RoleID == id);

            if (role == null)
                return NotFound();

            var dto = new RoleDto
            {
                RoleID = role.RoleID,
                RoleName = role.RoleName
                // Nếu có thêm trường trong DTO thì bổ sung ở đây
            };

            return Ok(dto);
        }

        /// <summary>
        /// [POST] Tạo mới một role. Chỉ Admin có quyền tạo.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<RoleDto>> PostRole([FromBody] RoleDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Kiểm tra trùng tên role
            if (await _context.Roles.AnyAsync(r => r.RoleName == dto.RoleName))
                return Conflict(new { Message = "Role đã tồn tại." });

            var role = new Role
            {
                RoleName = dto.RoleName
                // Nếu có thêm trường thì bổ sung ở đây
            };

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            var resultDto = new RoleDto
            {
                RoleID = role.RoleID,
                RoleName = role.RoleName
            };

            return CreatedAtAction(nameof(GetRole), new { id = role.RoleID }, resultDto);
        }

        /// <summary>
        /// [PUT] Cập nhật thông tin role theo ID. Chỉ Admin có quyền sửa.
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutRole(int id, [FromBody] RoleDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var role = await _context.Roles.FindAsync(id);
            if (role == null)
                return NotFound();

            // Kiểm tra trùng tên role
            if (await _context.Roles.AnyAsync(r => r.RoleName == dto.RoleName && r.RoleID != id))
                return Conflict(new { Message = "Tên role đã tồn tại." });

            role.RoleName = dto.RoleName;
            // Nếu có thêm trường thì bổ sung cập nhật ở đây

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// [DELETE] Xóa role theo ID. Chỉ Admin có quyền xóa.
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
                return NotFound();

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}