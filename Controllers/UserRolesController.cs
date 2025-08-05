using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftManagement.Data;
using ShiftManagement.DTOs;

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

        // POST: api/UserRoles
        [HttpPost]
        public async Task<IActionResult> AssignRole([FromBody] UserRoleDto dto)
        {
            // kiểm tra user + role tồn tại
            if (!await _context.Users.AnyAsync(u => u.UserID == dto.UserID))
                return NotFound($"User {dto.UserID} not found");
            if (!await _context.Roles.AnyAsync(r => r.RoleID == dto.RoleID))
                return NotFound($"Role {dto.RoleID} not found");

            // kiểm tra đã gán chưa
            bool exists = await _context.UserRoles
                .AnyAsync(ur => ur.UserID == dto.UserID && ur.RoleID == dto.RoleID);
            if (exists)
                return BadRequest("User already has this role");

            // gán
            _context.UserRoles.Add(new Models.UserRole
            {
                UserID = dto.UserID,
                RoleID = dto.RoleID
            });
            await _context.SaveChangesAsync();
            return Ok("Role assigned successfully");
        }

        // GET: api/UserRoles/user/5
        [HttpGet("user/{userId:int}")]
        public async Task<IActionResult> GetUserRoles(int userId)
        {
            if (!await _context.Users.AnyAsync(u => u.UserID == userId))
                return NotFound($"User {userId} not found");

            var roles = await _context.UserRoles
                .Where(ur => ur.UserID == userId)
                .Include(ur => ur.Role)
                .Select(ur => new {
                    ur.RoleID,
                    ur.Role.RoleName
                })
                .ToListAsync();

            return Ok(roles);
        }

        // DELETE: api/UserRoles
        [HttpDelete]
        public async Task<IActionResult> RemoveRole([FromBody] UserRoleDto dto)
        {
            var entry = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserID == dto.UserID && ur.RoleID == dto.RoleID);
            if (entry == null)
                return NotFound("This role assignment does not exist");

            _context.UserRoles.Remove(entry);
            await _context.SaveChangesAsync();
            return Ok("Role removed successfully");
        }
    }
}
