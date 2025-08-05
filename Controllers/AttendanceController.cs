using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftManagement.Data;
using ShiftManagement.Models;

namespace ShiftManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceController : ControllerBase
    {
        private readonly ShiftManagementContext _context;

        public AttendanceController(ShiftManagementContext context)
        {
            _context = context;
        }

        // GET: api/Attendance/report?departmentId=1&period=2025-08
        [HttpGet("report")]
        public async Task<ActionResult> GetAttendanceReport([FromQuery] int departmentId, [FromQuery] string period)
        {
            if (!DateTime.TryParse(period + "-01", out DateTime monthStart))
                return BadRequest("Invalid period format. Use yyyy-MM");

            var monthEnd = monthStart.AddMonths(1);

            var report = await _context.ShiftSchedules
                .Include(s => s.Employee)
                .Include(s => s.ShiftScheduleDetails)
                .Where(s => s.DepartmentID == departmentId && s.Date >= monthStart && s.Date < monthEnd)
                .GroupBy(s => new { s.Employee.UserID, s.Employee.FullName })
                .Select(g => new
                {
                    EmployeeID = g.Key.UserID,
                    Name = g.Key.FullName,
                    TotalWorkUnit = g.SelectMany(s => s.ShiftScheduleDetails).Sum(d => d.WorkUnit)
                })
                .ToListAsync();

            return Ok(report);
        }

        // GET: api/Attendance/my?period=2025-08
        [HttpGet("my")]
        public async Task<ActionResult> GetMyAttendance([FromQuery] string period)
        {
            if (!DateTime.TryParse(period + "-01", out DateTime monthStart))
                return BadRequest("Invalid period format. Use yyyy-MM");

            var monthEnd = monthStart.AddMonths(1);

            // Giả sử UserID lấy từ token hoặc tạm hardcode
            int currentUserId = 1;

            var myReport = await _context.ShiftSchedules
                .Include(s => s.ShiftScheduleDetails)
                .Where(s => s.EmployeeID == currentUserId && s.Date >= monthStart && s.Date < monthEnd)
                .Select(s => new
                {
                    s.Date,
                    TotalWorkUnit = s.ShiftScheduleDetails.Sum(d => d.WorkUnit)
                })
                .ToListAsync();

            return Ok(myReport);
        }
    }
}
