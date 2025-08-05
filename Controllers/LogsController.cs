using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftManagement.Data;
using ShiftManagement.DTOs;
using ShiftManagement.Models;
using Microsoft.AspNetCore.Authorization;

namespace ShiftManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        private readonly ShiftManagementContext _context;

        public LogsController(ShiftManagementContext context)
        {
            _context = context;
        }

        /// <summary>
        /// [GET] Lấy danh sách log, hỗ trợ tìm kiếm theo keyword, lọc theo User, phân trang.
        /// Chỉ tài khoản đã đăng nhập mới truy cập được.
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<LogDto>>> GetLogs(
            [FromQuery] string? search = "",
            [FromQuery] int? userId = null,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
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
                    UserID = l.UserID ?? 0, // SỬA: ép kiểu từ int? sang int
                    Username = l.User != null ? l.User.Username : "",
                    Action = l.Action,
                    Description = l.Description,
                    Timestamp = l.Timestamp
                })
                .ToListAsync();

            return Ok(logs);
        }

        /// <summary>
        /// [GET] Lấy chi tiết một log theo ID. Chỉ tài khoản đã đăng nhập mới truy cập được.
        /// </summary>
        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<ActionResult<LogDto>> GetLog(int id)
        {
            var log = await _context.Logs
                .AsNoTracking()
                .Include(l => l.User)
                .FirstOrDefaultAsync(l => l.LogID == id);

            if (log == null)
                return NotFound();

            var dto = new LogDto
            {
                LogID = log.LogID,
                UserID = log.UserID ?? 0, // SỬA: ép kiểu từ int? sang int
                Username = log.User != null ? log.User.Username : "",
                Action = log.Action,
                Description = log.Description,
                Timestamp = log.Timestamp
            };

            return Ok(dto);
        }

        /// <summary>
        /// [POST] Tạo mới một log. Chỉ Admin hoặc hệ thống được tạo log thủ công.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<LogDto>> PostLog([FromBody] LogCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Nếu UserID không tồn tại, không cho tạo log.
            if (!await _context.Users.AnyAsync(u => u.UserID == dto.UserID))
                return NotFound(new { Message = "Người dùng không tồn tại." });

            var log = new Log
            {
                UserID = dto.UserID,
                Action = dto.Action,
                Description = dto.Description,
                Timestamp = dto.Timestamp ?? DateTime.UtcNow
            };

            _context.Logs.Add(log);
            await _context.SaveChangesAsync();

            // Lấy lại thông tin username cho DTO trả về
            var user = await _context.Users.FindAsync(dto.UserID);

            var resultDto = new LogDto
            {
                LogID = log.LogID,
                UserID = log.UserID ?? 0, // SỬA: ép kiểu từ int? sang int
                Username = user?.Username ?? "",
                Action = log.Action,
                Description = log.Description,
                Timestamp = log.Timestamp
            };

            return CreatedAtAction(nameof(GetLog), new { id = log.LogID }, resultDto);
        }

        /// <summary>
        /// [DELETE] Xóa log theo ID. Chỉ Admin có quyền xóa.
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteLog(int id)
        {
            var log = await _context.Logs.FindAsync(id);
            if (log == null)
                return NotFound();

            _context.Logs.Remove(log);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}