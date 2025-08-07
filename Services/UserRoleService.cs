using Microsoft.EntityFrameworkCore;
using ShiftManagement.Data;
using ShiftManagement.DTOs;
using ShiftManagement.Models;

namespace ShiftManagement.Services
{
    public class UserRoleService
    {
        private readonly ShiftManagementContext _context;

        public UserRoleService(ShiftManagementContext context)
        {
            _context = context;
        }

        public async Task<List<UserRoleDto>> GetUserRolesAsync(int? userId = null, int? roleId = null)
        {
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

            return result;
        }

        public async Task<(UserRoleDto? Dto, string? Error)> AddUserRoleAsync(UserRoleCreateDto dto)
        {
            if (!await _context.Users.AnyAsync(u => u.UserID == dto.UserID))
                return (null, "Không tìm thấy người dùng.");

            if (!await _context.Roles.AnyAsync(r => r.RoleID == dto.RoleID))
                return (null, "Không tìm thấy role.");

            if (await _context.UserRoles.AnyAsync(ur => ur.UserID == dto.UserID && ur.RoleID == dto.RoleID))
                return (null, "User đã có role này.");

            var entity = new UserRole
            {
                UserID = dto.UserID,
                RoleID = dto.RoleID
            };

            _context.UserRoles.Add(entity);
            await _context.SaveChangesAsync();

            var role = await _context.Roles.FindAsync(dto.RoleID);
            var user = await _context.Users.FindAsync(dto.UserID);

            var resultDto = new UserRoleDto
            {
                UserID = dto.UserID,
                Username = user?.Username ?? "",
                RoleID = dto.RoleID,
                RoleName = role?.RoleName ?? ""
            };

            return (resultDto, null);
        }

        public async Task<UserRoleDto?> GetUserRoleAsync(int userId, int roleId)
        {
            var ur = await _context.UserRoles
                .Include(x => x.User)
                .Include(x => x.Role)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserID == userId && x.RoleID == roleId);

            if (ur == null)
                return null;

            return new UserRoleDto
            {
                UserID = ur.UserID,
                Username = ur.User.Username,
                RoleID = ur.RoleID,
                RoleName = ur.Role.RoleName
            };
        }

        public async Task<bool> DeleteUserRoleAsync(int userId, int roleId)
        {
            var ur = await _context.UserRoles
                .FirstOrDefaultAsync(x => x.UserID == userId && x.RoleID == roleId);

            if (ur == null)
                return false;

            _context.UserRoles.Remove(ur);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<string?> UpdateUserRoleAsync(int userId, int roleId, UserRoleUpdateDto dto)
        {
            var ur = await _context.UserRoles
                .FirstOrDefaultAsync(x => x.UserID == userId && x.RoleID == roleId);

            if (ur == null)
                return "Không tìm thấy user-role.";

            if (!await _context.Roles.AnyAsync(r => r.RoleID == dto.NewRoleID))
                return "Role mới không tồn tại.";

            ur.RoleID = dto.NewRoleID;
            await _context.SaveChangesAsync();

            return null;
        }
    }
}