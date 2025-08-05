using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftManagement.Data;
using ShiftManagement.DTOs;
using ShiftManagement.Models;

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

        // GET: api/Roles
        // Trả về list RoleDto (chỉ có ID + Name)
        [HttpGet]
        public async Task<ActionResult<List<RoleDto>>> GetRoles()
        {
            var list = await _context.Roles
                .AsNoTracking()
                .Select(r => new RoleDto
                {
                    RoleID = r.RoleID,
                    RoleName = r.RoleName
                })
                .ToListAsync();

            return Ok(list);
        }

        // GET: api/Roles/5
        // Trả về RoleDto hoặc RoleWithUsersDto nếu muốn kèm username
        [HttpGet("{id:int}")]
        public async Task<ActionResult<RoleWithUsersDto>> GetRole(int id)
        {
            var role = await _context.Roles
                .AsNoTracking()
                // Include userRoles để lấy username nếu cần
                .Include(r => r.UserRoles!)
                    .ThenInclude(ur => ur.User)
                .FirstOrDefaultAsync(r => r.RoleID == id);

            if (role == null)
                return NotFound();

            var dto = new RoleWithUsersDto
            {
                RoleID = role.RoleID,
                RoleName = role.RoleName,
                Usernames = role.UserRoles!
                    .Select(ur => ur.User!.Username)
                    .ToList()
            };

            return Ok(dto);
        }

        // POST: api/Roles
        // Nhận vào RoleCreateDto, trả về RoleDto
        [HttpPost]
        public async Task<ActionResult<RoleDto>> PostRole([FromBody] RoleCreateDto dto)
        {
            // Validate
            if (string.IsNullOrWhiteSpace(dto.RoleName))
                return BadRequest("RoleName is required.");

            // Tránh trùng
            if (await _context.Roles.AnyAsync(r => r.RoleName == dto.RoleName))
                return BadRequest("RoleName already exists.");

            // Tạo mới
            var role = new Role { RoleName = dto.RoleName };
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            var result = new RoleDto
            {
                RoleID = role.RoleID,
                RoleName = role.RoleName
            };

            return CreatedAtAction(nameof(GetRole), new { id = role.RoleID }, result);
        }

        // DELETE: api/Roles/5
        [HttpDelete("{id:int}")]
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
