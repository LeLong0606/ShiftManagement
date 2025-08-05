using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftManagement.Data;
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

        // GET: api/Logs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Log>>> GetLogs([FromQuery] int? userId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var query = _context.Logs
                .Include(l => l.User)
                .AsQueryable();

            if (userId.HasValue)
                query = query.Where(l => l.UserID == userId.Value);

            if (from.HasValue)
                query = query.Where(l => l.Timestamp >= from.Value);

            if (to.HasValue)
                query = query.Where(l => l.Timestamp <= to.Value);

            return await query.OrderByDescending(l => l.Timestamp).ToListAsync();
        }

        // POST: api/Logs
        [HttpPost]
        public async Task<IActionResult> CreateLog(Log log)
        {
            _context.Logs.Add(log);
            await _context.SaveChangesAsync();
            return Ok("Log created");
        }
    }
}
