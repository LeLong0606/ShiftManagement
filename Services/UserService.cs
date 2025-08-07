using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ShiftManagement.Data;
using ShiftManagement.DTOs;
using ShiftManagement.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ShiftManagement.Services
{
    public class UserService
    {
        private readonly ShiftManagementContext _context;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _config;

        private const int DefaultPageSize = 50;
        private const string UsersCachePrefix = "Users_";
        private const string UserCachePrefix = "User_";

        public UserService(
            ShiftManagementContext context,
            IMemoryCache cache,
            IConfiguration config)
        {
            _context = context;
            _cache = cache;
            _config = config;
        }

        public async Task<(bool Cached, List<UserDto> Data)> GetUsersAsync(string? search, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = DefaultPageSize;

            string cacheKey = $"{UsersCachePrefix}{search}_{page}_{pageSize}";
            if (_cache.TryGetValue(cacheKey, out List<UserDto> cached))
                return (true, cached);

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
            return (false, result);
        }

        public async Task<UserDto?> GetUserAsync(int id)
        {
            string cacheKey = $"{UserCachePrefix}{id}";
            if (_cache.TryGetValue(cacheKey, out UserDto dto))
                return dto;

            var user = await _context.Users
                .AsNoTracking()
                .Include(u => u.Department)
                .Include(u => u.Store)
                .FirstOrDefaultAsync(u => u.UserID == id);

            if (user == null) return null;

            dto = MapToDto(user);
            _cache.Set(cacheKey, dto, TimeSpan.FromMinutes(5));
            return dto;
        }

        public async Task<(bool Success, string? Error, int UserID, string? Username)> CreateUserAsync(UserCreateDto input)
        {
            if (await _context.Users.AnyAsync(u => u.Username == input.Username))
                return (false, "Tên đăng nhập đã tồn tại.", 0, null);

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

            return (true, null, entity.UserID, entity.Username);
        }

        public async Task<(bool Success, string? Error)> UpdateUserAsync(int id, UserUpdateDto input, ClaimsPrincipal userClaims)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return (false, "NotFound");

            var userIdClaim = userClaims.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!userClaims.IsInRole("Admin") && userIdClaim != user.UserID.ToString())
                return (false, "Forbid");

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

            return (true, null);
        }

        public async Task<(bool Success, string? Error)> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return (false, "NotFound");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            _cache.Remove($"{UserCachePrefix}{id}");
            RemoveAllUserListCache();

            return (true, null);
        }

        public async Task<(bool Success, string? Error)> ChangePasswordAsync(int id, ChangePasswordDto dto, ClaimsPrincipal userClaims)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return (false, "NotFound");

            var userIdClaim = userClaims.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim != user.UserID.ToString())
                return (false, "Forbid");

            if (!BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.PasswordHash))
                return (false, "Mật khẩu cũ không đúng.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            _cache.Remove($"{UserCachePrefix}{id}");

            return (true, null);
        }

        public async Task<(bool Success, bool? Status, string? Error)> ToggleLockAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return (false, null, "NotFound");

            user.Status = !user.Status;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            _cache.Remove($"{UserCachePrefix}{id}");

            return (true, user.Status, null);
        }

        public async Task<(bool Success, string? Error)> AddRoleAsync(int id, int roleId)
        {
            if (!await _context.Users.AnyAsync(u => u.UserID == id))
                return (false, "Không tìm thấy người dùng.");

            if (!await _context.Roles.AnyAsync(r => r.RoleID == roleId))
                return (false, "Không tìm thấy role.");

            if (await _context.UserRoles.AnyAsync(ur => ur.UserID == id && ur.RoleID == roleId))
                return (false, "User đã có role này.");

            _context.UserRoles.Add(new UserRole { UserID = id, RoleID = roleId });
            await _context.SaveChangesAsync();

            return (true, null);
        }

        public async Task<(bool Success, string? Error)> RemoveRoleAsync(int id, int roleId)
        {
            var ur = await _context.UserRoles
                .FirstOrDefaultAsync(x => x.UserID == id && x.RoleID == roleId);

            if (ur == null)
                return (false, "NotFound");

            _context.UserRoles.Remove(ur);
            await _context.SaveChangesAsync();

            return (true, null);
        }

        public async Task<List<string>> GetUserRolesAsync(int id)
        {
            return await _context.UserRoles
                .Where(ur => ur.UserID == id)
                .Include(ur => ur.Role)
                .Select(ur => ur.Role.RoleName)
                .ToListAsync();
        }

        public async Task<string?> LoginAsync(LoginDto dto)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == dto.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return null;

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

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

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

        private void RemoveAllUserListCache()
        {
            // TODO: Cần bổ sung logic xóa cache khi dùng cache phân tán.
        }
    }
}