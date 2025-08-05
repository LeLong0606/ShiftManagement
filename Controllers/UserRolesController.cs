using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftManagement.Data;
using ShiftManagement.Models;

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
        public async Task<IActionResult> AssignRole([FromBody] UserRole userRole)
        {
            var exists = await _context.UserRoles
                .AnyAsync(ur => ur.UserID == userRole.UserID && ur.RoleID == userRole.RoleID);

            if (exists)
                return BadRequest("User already has this role");

            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();
            return Ok("Role assigned successfully");
        }

        // GET: api/UserRoles/user/5
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserRoles(int userId)
        {
            var roles = await _context.UserRoles
                .Include(ur => ur.Role)
                .Where(ur => ur.UserID == userId)
                .Select(ur => ur.Role.RoleName)
                .ToListAsync();

            return Ok(roles);
        }

        // DELETE: api/UserRoles
        [HttpDelete]
        public async Task<IActionResult> RemoveRole([FromBody] UserRole userRole)
        {
            var entry = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserID == userRole.UserID && ur.RoleID == userRole.RoleID);

            if (entry == null)
                return NotFound();

            _context.UserRoles.Remove(entry);
            await _context.SaveChangesAsync();
            return Ok("Role removed successfully");
        }
    }
}
