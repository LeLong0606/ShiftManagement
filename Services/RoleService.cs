using Microsoft.EntityFrameworkCore;
using ShiftManagement.Data;
using ShiftManagement.DTOs;
using ShiftManagement.Models;

namespace ShiftManagement.Services
{
    public class RoleService
    {
        private readonly ShiftManagementContext _context;

        public RoleService(ShiftManagementContext context)
        {
            _context = context;
        }

        public async Task<List<RoleDto>> GetRolesAsync(string? search = "", int page = 1, int pageSize = 50)
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
                })
                .ToListAsync();

            return roles;
        }

        public async Task<RoleDto?> GetRoleAsync(int id)
        {
            var role = await _context.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.RoleID == id);

            if (role == null)
                return null;

            return new RoleDto
            {
                RoleID = role.RoleID,
                RoleName = role.RoleName
            };
        }

        public async Task<(RoleDto? Dto, string? Error)> CreateRoleAsync(RoleDto dto)
        {
            if (await _context.Roles.AnyAsync(r => r.RoleName == dto.RoleName))
                return (null, "Role đã tồn tại.");

            var role = new Role
            {
                RoleName = dto.RoleName
            };

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            var resultDto = new RoleDto
            {
                RoleID = role.RoleID,
                RoleName = role.RoleName
            };

            return (resultDto, null);
        }

        public async Task<string?> UpdateRoleAsync(int id, RoleDto dto)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
                return "Không tìm thấy role.";

            if (await _context.Roles.AnyAsync(r => r.RoleName == dto.RoleName && r.RoleID != id))
                return "Tên role đã tồn tại.";

            role.RoleName = dto.RoleName;
            await _context.SaveChangesAsync();
            return null;
        }

        public async Task<bool> DeleteRoleAsync(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
                return false;

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}