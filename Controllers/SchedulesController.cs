using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftManagement.Data;
using ShiftManagement.Models;

namespace ShiftManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SchedulesController : ControllerBase
    {
        private readonly ShiftManagementContext _context;

        public SchedulesController(ShiftManagementContext context)
        {
            _context = context;
        }

        // GET: api/Schedules?from=2025-08-01&to=2025-08-31&departmentId=1
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ShiftSchedule>>> GetSchedules(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] int? departmentId)
        {
            var query = _context.ShiftSchedules
                .Include(s => s.Employee)
                .Include(s => s.Department)
                .Include(s => s.Store)
                .Include(s => s.ShiftScheduleDetails)
                .AsQueryable();

            if (from.HasValue)
                query = query.Where(s => s.Date >= from.Value);

            if (to.HasValue)
                query = query.Where(s => s.Date <= to.Value);

            if (departmentId.HasValue)
                query = query.Where(s => s.DepartmentID == departmentId.Value);

            return await query.ToListAsync();
        }

        // POST: api/Schedules
        [HttpPost]
        public async Task<ActionResult<ShiftSchedule>> PostSchedule(ShiftSchedule schedule)
        {
            _context.ShiftSchedules.Add(schedule);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetSchedules), new { id = schedule.ScheduleID }, schedule);
        }

        // PUT: api/Schedules/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSchedule(int id, ShiftSchedule schedule)
        {
            if (id != schedule.ScheduleID)
                return BadRequest();

            _context.Entry(schedule).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Schedules/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            var schedule = await _context.ShiftSchedules.FindAsync(id);
            if (schedule == null)
                return NotFound();

            _context.ShiftSchedules.Remove(schedule);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
