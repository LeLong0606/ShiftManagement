using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using ShiftManagement.Data;
using ShiftManagement.DTOs;
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

        private const int DefaultPageSize = 50;
        private const string UsersCachePrefix = "Users_";
        private const string UserCachePrefix = "User_";

        public UsersController(
            ShiftManagementContext context,
            IMemoryCache cache,
            IConfiguration config)
        {
            _context = context;
            _cache = cache;
            _config = config;
        }

        /// <summary>
        /// Gets a paginated list of users, optionally filtered by search term.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> GetUsers(
            [FromQuery] string? search = "",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = DefaultPageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = DefaultPageSize;

            string cacheKey = $"{UsersCachePrefix}{search}_{page}_{pageSize}";
            if (_cache.TryGetValue(cacheKey, out List<UserDto> cached))
                return Ok(new { Cached = true, Data = cached });

            // Chú ý: Include phải được gọi liên tiếp, không gán lại cho biến IQueryable
            IQueryable<User> query = _context.Users.AsNoTracking();

            if (!string.IsNullOrEmpty(search))
            {
                string pattern = $"%{search}%";
                query = query.Where(u =>
                    EF.Functions.Like(u.Username, pattern) ||
                    (u.Email != null && EF.Functions.Like(u.Email, pattern)) ||
                    (u.FullName != null && EF.Functions.Like(u.FullName, pattern))
                );
            }

            // Include các bảng liên quan
            query = query
                .Include(u => u.Department)
                .Include(u => u.Store);

            var result = await query
                .OrderBy(u => u.UserID)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserDto
                {
                    UserID = u.UserID,
                    Username = u.Username,
                    FullName = u.FullName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    DepartmentID = u.DepartmentID,
                    DepartmentName = u.Department != null ? u.Department.DepartmentName : null,
                    StoreID = u.StoreID,
                    StoreName = u.Store != null ? u.Store.StoreName : null,
                    Status = u.Status,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt
                })
                .ToListAsync();

            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(2));
            return Ok(new { Cached = false, Data = result });
        }

        /// <summary>
        /// Gets a single user by ID.
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            string cacheKey = $"{UserCachePrefix}{id}";
            if (_cache.TryGetValue(cacheKey, out UserDto dto))
                return Ok(dto);

            var user = await _context.Users
                .AsNoTracking()
                .Include(u => u.Department)
                .Include(u => u.Store)
                .FirstOrDefaultAsync(u => u.UserID == id);

            if (user == null) return NotFound();

            dto = MapToDto(user);
            _cache.Set(cacheKey, dto, TimeSpan.FromMinutes(5));
            return Ok(dto);
        }

        /// <summary>
        /// Creates a new user.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> PostUser([FromBody] UserCreateDto input)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            if (await _context.Users.AnyAsync(u => u.Username == input.Username))
                return Conflict(new { Message = "Username already exists." });

            var entity = new User
            {
                Username = input.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(input.Password),
                FullName = input.FullName,
                Email = input.Email,
                PhoneNumber = input.PhoneNumber,
                DepartmentID = input.DepartmentID,
                StoreID = input.StoreID,
                Status = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(entity);
            await _context.SaveChangesAsync();

            RemoveAllUserListCache();

            return CreatedAtAction(nameof(GetUser),
                new { id = entity.UserID },
                new { entity.UserID, entity.Username });
        }

        /// <summary>
        /// Updates an existing user.
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutUser(int id, [FromBody] UserUpdateDto input)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.FullName = input.FullName;
            user.Email = input.Email;
            user.PhoneNumber = input.PhoneNumber;
            user.DepartmentID = input.DepartmentID;
            user.StoreID = input.StoreID;
            user.Status = input.Status;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _cache.Remove($"{UserCachePrefix}{id}");
            RemoveAllUserListCache();

            return NoContent();
        }

        /// <summary>
        /// Deletes a user.
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            _cache.Remove($"{UserCachePrefix}{id}");
            RemoveAllUserListCache();

            return NoContent();
        }

        /// <summary>
        /// Changes user password.
        /// </summary>
        [HttpPatch("{id:int}/changepassword")]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            if (!BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.PasswordHash))
                return BadRequest(new { Message = "Old password incorrect." });

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            _cache.Remove($"{UserCachePrefix}{id}");

            return NoContent();
        }

        /// <summary>
        /// Toggles user lock status.
        /// </summary>
        [HttpPatch("{id:int}/lock")]
        public async Task<IActionResult> ToggleLock(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.Status = !user.Status;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            _cache.Remove($"{UserCachePrefix}{id}");

            return Ok(new { user.UserID, user.Status });
        }

        /// <summary>
        /// Adds a role to a user.
        /// </summary>
        [HttpPost("{id:int}/roles")]
        public async Task<IActionResult> AddRole(int id, [FromBody] int roleId)
        {
            if (!await _context.Users.AnyAsync(u => u.UserID == id))
                return NotFound(new { Message = "User not found." });

            if (!await _context.Roles.AnyAsync(r => r.RoleID == roleId))
                return NotFound(new { Message = "Role not found." });

            if (await _context.UserRoles.AnyAsync(ur => ur.UserID == id && ur.RoleID == roleId))
                return Conflict(new { Message = "Role already assigned." });

            _context.UserRoles.Add(new UserRole { UserID = id, RoleID = roleId });
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Role added." });
        }

        /// <summary>
        /// Removes a role from a user.
        /// </summary>
        [HttpDelete("{id:int}/roles/{roleId:int}")]
        public async Task<IActionResult> RemoveRole(int id, int roleId)
        {
            var ur = await _context.UserRoles
                .FirstOrDefaultAsync(x => x.UserID == id && x.RoleID == roleId);
            if (ur == null) return NotFound();

            _context.UserRoles.Remove(ur);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Role removed." });
        }

        /// <summary>
        /// Gets the roles of a user.
        /// </summary>
        [HttpGet("{id:int}/roles")]
        public async Task<IActionResult> GetUserRoles(int id)
        {
            var roles = await _context.UserRoles
                .Where(ur => ur.UserID == id)
                .Include(ur => ur.Role)
                .Select(ur => ur.Role.RoleName)
                .ToListAsync();

            return Ok(roles);
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token.
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == dto.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized(new { Message = "Invalid username or password." });

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            };
            claims.AddRange(user.UserRoles.Select(ur =>
                new Claim(ClaimTypes.Role, ur.Role.RoleName)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds
            );

            return Ok(new { Token = new JwtSecurityTokenHandler().WriteToken(token) });
        }

        #region Helpers

        /// <summary>
        /// Maps a User entity to UserDto.
        /// </summary>
        private static UserDto MapToDto(User user) =>
            new UserDto
            {
                UserID = user.UserID,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                DepartmentID = user.DepartmentID,
                DepartmentName = user.Department?.DepartmentName,
                StoreID = user.StoreID,
                StoreName = user.Store?.StoreName,
                Status = user.Status,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };

        /// <summary>
        /// Removes all user list caches.
        /// </summary>
        private void RemoveAllUserListCache()
        {
            // IMemoryCache không hỗ trợ xóa theo wildcard.
            // Có thể lưu lại các cache key đã set để xóa, hoặc dùng cache phân tán như Redis.
            // Ở đây để trống (hoặc có thể thêm logic tracking key nếu cần).
        }

        #endregion
    }
}