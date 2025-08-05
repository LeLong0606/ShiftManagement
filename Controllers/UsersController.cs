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

        // compiled query cho UserDto
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
                            DepartmentID = u.DepartmentID,
                            DepartmentName = u.Department != null ? u.Department.DepartmentName : null,
                            StoreID = u.StoreID,
                            StoreName = u.Store != null ? u.Store.StoreName : null,
                            Status = u.Status,
                            CreatedAt = u.CreatedAt,
                            UpdatedAt = u.UpdatedAt
                        })
            );

        public UsersController(
            ShiftManagementContext context,
            IMemoryCache cache,
            IConfiguration config)
        {
            _context = context;
            _cache = cache;
            _config = config;
        }

        // GET: api/Users
        [HttpGet]
        public ActionResult GetUsers(
            [FromQuery] string? search = "",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            string cacheKey = $"Users_{search}_{page}_{pageSize}";

            if (_cache.TryGetValue(cacheKey, out List<UserDto> cached))
                return Ok(new { Cached = true, Data = cached });

            var result = _compiledQuery(
                _context,
                search,
                (page - 1) * pageSize,
                pageSize)
              .ToList();

            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(2));
            return Ok(new { Cached = false, Data = result });
        }

        // GET: api/Users/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            string cacheKey = $"User_{id}";
            if (_cache.TryGetValue(cacheKey, out UserDto dto))
                return Ok(dto);

            dto = await _context.Users
                .AsNoTracking()
                .Where(u => u.UserID == id)
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
                .FirstOrDefaultAsync();

            if (dto == null) return NotFound();

            _cache.Set(cacheKey, dto, TimeSpan.FromMinutes(5));
            return Ok(dto);
        }

        // POST: api/Users
        [HttpPost]
        public async Task<ActionResult> PostUser([FromBody] UserCreateDto input)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (await _context.Users.AnyAsync(u => u.Username == input.Username))
                return BadRequest("Username already exists.");

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

            _cache.Remove($"Users_*");
            return CreatedAtAction(nameof(GetUser),
                new { id = entity.UserID },
                new { entity.UserID, entity.Username });
        }

        // PUT: api/Users/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutUser(
            int id,
            [FromBody] UserUpdateDto input)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

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
            _cache.Remove($"User_{id}");
            return NoContent();
        }

        // DELETE: api/Users/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            _cache.Remove($"User_{id}");
            return NoContent();
        }

        // PATCH: api/Users/{id}/changepassword
        [HttpPatch("{id:int}/changepassword")]
        public async Task<IActionResult> ChangePassword(
            int id,
            [FromBody] ChangePasswordDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            if (!BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.PasswordHash))
                return BadRequest("Old password incorrect.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // PATCH: api/Users/{id}/lock
        [HttpPatch("{id:int}/lock")]
        public async Task<IActionResult> ToggleLock(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.Status = !user.Status;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(new { user.UserID, user.Status });
        }

        // POST: api/Users/{id}/roles
        [HttpPost("{id:int}/roles")]
        public async Task<IActionResult> AddRole(int id, [FromBody] int roleId)
        {
            if (!await _context.Users.AnyAsync(u => u.UserID == id))
                return NotFound("User not found.");

            if (!await _context.Roles.AnyAsync(r => r.RoleID == roleId))
                return NotFound("Role not found.");

            if (await _context.UserRoles.AnyAsync(ur => ur.UserID == id && ur.RoleID == roleId))
                return BadRequest("Role already assigned.");

            _context.UserRoles.Add(new UserRole { UserID = id, RoleID = roleId });
            await _context.SaveChangesAsync();
            return Ok("Role added.");
        }

        // DELETE: api/Users/{id}/roles/{roleId}
        [HttpDelete("{id:int}/roles/{roleId:int}")]
        public async Task<IActionResult> RemoveRole(int id, int roleId)
        {
            var ur = await _context.UserRoles
                .FirstOrDefaultAsync(x => x.UserID == id && x.RoleID == roleId);
            if (ur == null) return NotFound();

            _context.UserRoles.Remove(ur);
            await _context.SaveChangesAsync();
            return Ok("Role removed.");
        }

        // GET: api/Users/{id}/roles
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

        // POST: api/Users/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == dto.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Invalid username or password.");

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
    }
}
