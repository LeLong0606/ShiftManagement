using Microsoft.EntityFrameworkCore;
using ShiftManagement.Data;
using ShiftManagement.DTOs;
using ShiftManagement.Models;

namespace ShiftManagement.Services
{
    public class LogService
    {
        private readonly ShiftManagementContext _context;

        public LogService(ShiftManagementContext context)
        {
            _context = context;
        }

        public async Task<List<LogDto>> GetLogsAsync(
            string? search = "",
            int? userId = null,
            DateTime? from = null,
            DateTime? to = null,
            int page = 1,
            int pageSize = 50)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 50;

            var query = _context.Logs
                .AsNoTracking()
                .Include(l => l.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                string pattern = $"%{search}%";
                query = query.Where(l =>
                    EF.Functions.Like(l.Action, pattern) ||
                    EF.Functions.Like(l.Description ?? "", pattern));
            }

            if (userId.HasValue)
                query = query.Where(l => l.UserID == userId.Value);

            if (from.HasValue)
                query = query.Where(l => l.Timestamp >= from.Value);

            if (to.HasValue)
                query = query.Where(l => l.Timestamp <= to.Value);

            var logs = await query
                .OrderByDescending(l => l.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(l => new LogDto
                {
                    LogID = l.LogID,
                    UserID = l.UserID ?? 0,
                    Username = l.User != null ? l.User.Username : "",
                    Action = l.Action,
                    Description = l.Description,
                    Timestamp = l.Timestamp
                })
                .ToListAsync();

            return logs;
        }

        public async Task<LogDto?> GetLogAsync(int id)
        {
            var log = await _context.Logs
                .AsNoTracking()
                .Include(l => l.User)
                .FirstOrDefaultAsync(l => l.LogID == id);

            if (log == null) return null;

            return new LogDto
            {
                LogID = log.LogID,
                UserID = log.UserID ?? 0,
                Username = log.User != null ? log.User.Username : "",
                Action = log.Action,
                Description = log.Description,
                Timestamp = log.Timestamp
            };
        }

        public async Task<(LogDto? Dto, string? Error)> CreateLogAsync(LogCreateDto dto)
        {
            if (!await _context.Users.AnyAsync(u => u.UserID == dto.UserID))
                return (null, "Người dùng không tồn tại.");

            var log = new Log
            {
                UserID = dto.UserID,
                Action = dto.Action,
                Description = dto.Description,
                Timestamp = dto.Timestamp ?? DateTime.UtcNow
            };

            _context.Logs.Add(log);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(dto.UserID);

            var resultDto = new LogDto
            {
                LogID = log.LogID,
                UserID = log.UserID ?? 0,
                Username = user?.Username ?? "",
                Action = log.Action,
                Description = log.Description,
                Timestamp = log.Timestamp
            };

            return (resultDto, null);
        }

        public async Task<bool> DeleteLogAsync(int id)
        {
            var log = await _context.Logs.FindAsync(id);
            if (log == null)
                return false;

            _context.Logs.Remove(log);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}