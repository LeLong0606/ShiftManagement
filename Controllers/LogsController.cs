// Controllers/LogsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftManagement.Data;
using ShiftManagement.DTOs;
using ShiftManagement.Models;

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

        // GET: api/Logs?userId=3&from=2025-08-01&to=2025-08-31
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LogDto>>> GetLogs(
            [FromQuery] int? userId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var query = _context.Logs
                .AsNoTracking()
                .Include(l => l.User)
                .AsQueryable();

            if (userId.HasValue)
                query = query.Where(l => l.UserID == userId.Value);

            if (from.HasValue)
                query = query.Where(l => l.Timestamp >= from.Value);

            if (to.HasValue)
                query = query.Where(l => l.Timestamp <= to.Value);

            var list = await query
                .OrderByDescending(l => l.Timestamp)
                .Select(l => new LogDto
                {
                    LogID = l.LogID,
                    UserID = l.UserID ?? 0,
                    Username = l.User!.Username,
                    Action = l.Action,
                    Timestamp = l.Timestamp
                })
                .ToListAsync();

            return Ok(list);
        }

        // POST: api/Logs
        [HttpPost]
        public async Task<IActionResult> CreateLog([FromBody] LogCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // ensure the UserID is valid
            if (!await _context.Users.AnyAsync(u => u.UserID == dto.UserID))
                return BadRequest("UserID is invalid.");

            var entity = new Log
            {
                UserID = dto.UserID,       // now non-nullable
                Action = dto.Action,
                Timestamp = DateTime.UtcNow
            };

            _context.Logs.Add(entity);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetLogs),
                new { userId = entity.UserID },
                new { Message = "Log created", LogID = entity.LogID }
            );
        }
    }
}
