using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using ShiftManagement.Data;
using ShiftManagement.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ShiftManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ShiftManagementContext _context;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _config;

        public UsersController(ShiftManagementContext context, IMemoryCache cache, IConfiguration config)
        {
            _context = context;
            _cache = cache;
            _config = config;
        }

        // ========================
        // 1️⃣ COMPILED QUERY
        // ========================
        private static readonly Func<ShiftManagementContext, string?, int, int, IEnumerable<UserDto>>
            _compiledQuery = EF.CompileQuery(
                (ShiftManagementContext ctx, string? search, int skip, int take) =>
                    ctx.Users
                        .AsNoTracking()
                        .Where(u =>
                            string.IsNullOrEmpty(search)
                            || u.Username.Contains(search)
                            || (u.Email != null && u.Email.Contains(search))
                            || (u.FullName != null && u.FullName.Contains(search)))
                        .OrderBy(u => u.UserID)
                        .Skip(skip)
                        .Take(take)
                        .Select(u => new UserDto
                        {
                            UserID = u.UserID,
                            Username = u.Username,
                            FullName = u.FullName,
                            Email = u.Email,
                            PhoneNumber = u.PhoneNumber,
                            DepartmentName = u.Department != null ? u.Department.DepartmentName : null,
                            StoreName = u.Store != null ? u.Store.StoreName : null,
                            Status = u.Status
                        })
            );

        // ========================
        // 2️⃣ GET LIST
        // ========================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers(
            [FromQuery] string? search = "",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            string cacheKey = $"Users_{search}_{page}_{pageSize}";

            if (_cache.TryGetValue(cacheKey, out List<UserDto> cachedData))
                return Ok(new { Cached = true, Data = cachedData });

            List<UserDto> users = await Task.FromResult(
                _compiledQuery(_context, search ?? "", (page - 1) * pageSize, pageSize).ToList()
            );

            _cache.Set(cacheKey, users, TimeSpan.FromMinutes(2));
            return Ok(new { Cached = false, Data = users });
        }

        // ========================
        // 3️⃣ GET BY ID
        // ========================
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            string cacheKey = $"User_{id}";
            if (_cache.TryGetValue(cacheKey, out UserDto cachedUser))
                return Ok(cachedUser);

            var user = await _context.Users
                .AsNoTracking()
                .Where(u => u.UserID == id)
                .Select(u => new UserDto
                {
                    UserID = u.UserID,
                    Username = u.Username,
                    FullName = u.FullName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    DepartmentName = u.Department != null ? u.Department.DepartmentName : null,
                    StoreName = u.Store != null ? u.Store.StoreName : null,
                    Status = u.Status
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound();

            _cache.Set(cacheKey, user, TimeSpan.FromMinutes(5));
            return Ok(user);
        }

        // ========================
        // 4️⃣ CREATE
        // ========================
        [HttpPost]
        public async Task<ActionResult<User>> PostUser([FromBody] User user)
        {
            if (await _context.Users.AnyAsync(u => u.Username == user.Username))
                return BadRequest("Username already exists.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _cache.Remove("Users_*"); // clear cache list
            return CreatedAtAction(nameof(GetUser), new { id = user.UserID }, user);
        }

        // ========================
        // 5️⃣ UPDATE
        // ========================
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, [FromBody] User user)
        {
            if (id != user.UserID) return BadRequest();

            var existing = await _context.Users.FindAsync(id);
            if (existing == null) return NotFound();

            existing.FullName = user.FullName;
            existing.Email = user.Email;
            existing.PhoneNumber = user.PhoneNumber;
            existing.DepartmentID = user.DepartmentID;
            existing.StoreID = user.StoreID;
            existing.Status = user.Status;
            existing.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            _cache.Remove($"User_{id}");
            return NoContent();
        }

        // ========================
        // 6️⃣ DELETE
        // ========================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            _cache.Remove($"User_{id}");
            return NoContent();
        }

        // ========================
        // 7️⃣ CHANGE PASSWORD
        // ========================
        [HttpPatch("{id}/changepassword")]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            if (!BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.PasswordHash))
                return BadRequest("Old password incorrect.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ========================
        // 8️⃣ LOCK/UNLOCK
        // ========================
        [HttpPatch("{id}/lock")]
        public async Task<IActionResult> ToggleLock(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.Status = !user.Status;
            await _context.SaveChangesAsync();
            return Ok(new { user.UserID, user.Status });
        }

        // ========================
        // 9️⃣ ASSIGN ROLE
        // ========================
        [HttpPost("{id}/roles")]
        public async Task<IActionResult> AddRole(int id, [FromBody] int roleId)
        {
            if (!await _context.Users.AnyAsync(u => u.UserID == id)) return NotFound("User not found");
            if (!await _context.Roles.AnyAsync(r => r.RoleID == roleId)) return NotFound("Role not found");

            if (await _context.UserRoles.AnyAsync(ur => ur.UserID == id && ur.RoleID == roleId))
                return BadRequest("Role already assigned.");

            _context.UserRoles.Add(new UserRole { UserID = id, RoleID = roleId });
            await _context.SaveChangesAsync();
            return Ok("Role added.");
        }

        // ========================
        // 🔟 REMOVE ROLE
        // ========================
        [HttpDelete("{id}/roles/{roleId}")]
        public async Task<IActionResult> RemoveRole(int id, int roleId)
        {
            var userRole = await _context.UserRoles.FirstOrDefaultAsync(ur => ur.UserID == id && ur.RoleID == roleId);
            if (userRole == null) return NotFound();

            _context.UserRoles.Remove(userRole);
            await _context.SaveChangesAsync();
            return Ok("Role removed.");
        }

        // ========================
        // 11️⃣ GET ROLES
        // ========================
        [HttpGet("{id}/roles")]
        public async Task<IActionResult> GetUserRoles(int id)
        {
            var roles = await _context.UserRoles
                .Where(ur => ur.UserID == id)
                .Include(ur => ur.Role)
                .Select(ur => ur.Role.RoleName)
                .ToListAsync();

            return Ok(roles);
        }

        // ========================
        // 12️⃣ LOGIN -> JWT
        // ========================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Invalid username or password.");

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds
            );

            return Ok(new { Token = new JwtSecurityTokenHandler().WriteToken(token) });
        }
    }

    // ==========================
    // DTOs
    // ==========================
    public class UserDto
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string? DepartmentName { get; set; }
        public string? StoreName { get; set; }
        public bool Status { get; set; }
    }

    public class ChangePasswordDto
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }

    public class LoginDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
