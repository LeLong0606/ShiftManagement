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
using Microsoft.AspNetCore.Authorization;

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
        /// [GET] Lấy danh sách người dùng, có thể tìm kiếm theo keyword.
        /// Phân trang với page và pageSize.
        /// Chỉ truy cập khi đã đăng nhập.
        /// </summary>
        [HttpGet]
        [Authorize]
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

            IQueryable<User> query = _context.Users.AsNoTracking();

            // Nếu có search, áp dụng tìm kiếm theo Username, Email, FullName
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
        /// [GET] Lấy thông tin chi tiết một người dùng theo ID.
        /// Chỉ truy cập khi đã đăng nhập.
        /// </summary>
        [HttpGet("{id:int}")]
        [Authorize]
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
        /// [POST] Tạo mới một người dùng.
        /// Yêu cầu quyền Admin mới tạo được user.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> PostUser([FromBody] UserCreateDto input)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            // Kiểm tra trùng username
            if (await _context.Users.AnyAsync(u => u.Username == input.Username))
                return Conflict(new { Message = "Tên đăng nhập đã tồn tại." });

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
        /// [PUT] Cập nhật thông tin người dùng theo ID.
        /// Chỉ Admin hoặc chính user được sửa thông tin cá nhân.
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> PutUser(int id, [FromBody] UserUpdateDto input)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            // Chỉ cho phép admin hoặc chính user được sửa mình
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole("Admin") && userIdClaim != user.UserID.ToString())
                return Forbid();

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
        /// [DELETE] Xóa người dùng theo ID.
        /// Chỉ Admin có quyền xóa.
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
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
        /// [PATCH] Đổi mật khẩu cho user.
        /// Chỉ chính user mới được đổi mật khẩu của mình.
        /// </summary>
        [HttpPatch("{id:int}/changepassword")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim != user.UserID.ToString())
                return Forbid();

            if (!BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.PasswordHash))
                return BadRequest(new { Message = "Mật khẩu cũ không đúng." });

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            _cache.Remove($"{UserCachePrefix}{id}");

            return NoContent();
        }

        /// <summary>
        /// [PATCH] Khóa/mở khóa người dùng.
        /// Chỉ Admin có quyền khóa/mở.
        /// </summary>
        [HttpPatch("{id:int}/lock")]
        [Authorize(Roles = "Admin")]
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
        /// [POST] Gán role cho user.
        /// Chỉ Admin mới có quyền gán.
        /// </summary>
        [HttpPost("{id:int}/roles")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddRole(int id, [FromBody] int roleId)
        {
            if (!await _context.Users.AnyAsync(u => u.UserID == id))
                return NotFound(new { Message = "Không tìm thấy người dùng." });

            if (!await _context.Roles.AnyAsync(r => r.RoleID == roleId))
                return NotFound(new { Message = "Không tìm thấy role." });

            if (await _context.UserRoles.AnyAsync(ur => ur.UserID == id && ur.RoleID == roleId))
                return Conflict(new { Message = "User đã có role này." });

            _context.UserRoles.Add(new UserRole { UserID = id, RoleID = roleId });
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Đã thêm role cho user." });
        }

        /// <summary>
        /// [DELETE] Xóa role của user.
        /// Chỉ Admin mới có quyền xóa role.
        /// </summary>
        [HttpDelete("{id:int}/roles/{roleId:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveRole(int id, int roleId)
        {
            var ur = await _context.UserRoles
                .FirstOrDefaultAsync(x => x.UserID == id && x.RoleID == roleId);

            if (ur == null) return NotFound();

            _context.UserRoles.Remove(ur);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Đã xóa role của user." });
        }

        /// <summary>
        /// [GET] Lấy danh sách role của user.
        /// Chỉ truy cập khi đã đăng nhập.
        /// </summary>
        [HttpGet("{id:int}/roles")]
        [Authorize]
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
        /// [POST] Đăng nhập lấy JWT token, không yêu cầu phân quyền.
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == dto.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized(new { Message = "Tên đăng nhập hoặc mật khẩu không đúng." });

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
        /// Hàm chuyển đổi User entity sang UserDto.
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
        /// Xóa toàn bộ cache danh sách user (chưa hiện thực, cần Redis hoặc tracking key).
        /// </summary>
        private void RemoveAllUserListCache()
        {
            // TODO: Cần bổ sung logic xóa cache khi dùng cache phân tán.
        }

        #endregion
    }
}