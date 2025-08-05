using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftManagement.Data;
using ShiftManagement.DTOs;
using System.Security.Claims;

namespace ShiftManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AttendanceController : ControllerBase
    {
        private readonly ShiftManagementContext _context;

        public AttendanceController(ShiftManagementContext context)
        {
            _context = context;
        }

        // GET: api/Attendance/report?departmentId=1&period=2025-08
        [HttpGet("report")]
        [Authorize(Roles = "Admin,Director,TeamLeader")]
        public async Task<ActionResult<IEnumerable<AttendanceReportDto>>> GetAttendanceReport([FromQuery] int departmentId, [FromQuery] string period)
        {
            if (!DateTime.TryParse(period + "-01", out DateTime monthStart))
                return BadRequest("Invalid period format. Use yyyy-MM");

            var monthEnd = monthStart.AddMonths(1);

            var report = await _context.ShiftSchedules
                .AsNoTracking()
                .Where(s => s.DepartmentID == departmentId && s.Date >= monthStart && s.Date < monthEnd)
                .Select(s => new
                {
                    s.Employee.UserID,
                    s.Employee.FullName,
                    Details = s.ShiftScheduleDetails.Select(d => d.WorkUnit)
                })
                .ToListAsync();

            var dtoReport = report
                .GroupBy(s => new { s.UserID, s.FullName })
                .Select(g => new AttendanceReportDto
                {
                    EmployeeID = g.Key.UserID,
                    Name = g.Key.FullName,
                    TotalWorkUnit = g.SelectMany(x => x.Details).Sum()
                }).ToList();

            return Ok(dtoReport);
        }

        // GET: api/Attendance/my?period=2025-08
        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<MyAttendanceDto>>> GetMyAttendance([FromQuery] string period)
        {
            if (!DateTime.TryParse(period + "-01", out DateTime monthStart))
                return BadRequest("Invalid period format. Use yyyy-MM");

            var monthEnd = monthStart.AddMonths(1);

            // Lấy UserID từ token
            int currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (currentUserId == 0)
                return Unauthorized();

            var myReport = await _context.ShiftSchedules
                .AsNoTracking()
                .Where(s => s.EmployeeID == currentUserId && s.Date >= monthStart && s.Date < monthEnd)
                .Select(s => new MyAttendanceDto
                {
                    Date = s.Date,
                    TotalWorkUnit = s.ShiftScheduleDetails.Sum(d => d.WorkUnit)
                })
                .ToListAsync();

            return Ok(myReport);
        }
    }
}