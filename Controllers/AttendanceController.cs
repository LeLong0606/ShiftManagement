using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftManagement.DTOs;
using ShiftManagement.Services;
using System.Security.Claims;

namespace ShiftManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AttendanceController : ControllerBase
    {
        private readonly AttendanceService _attendanceService;

        public AttendanceController(AttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        // GET: api/Attendance/report?departmentId=1&period=2025-08
        [HttpGet("report")]
        [Authorize(Roles = "Admin,Director,TeamLeader")]
        public async Task<IActionResult> GetAttendanceReport([FromQuery] int departmentId, [FromQuery] string period)
        {
            try
            {
                var result = await _attendanceService.GetAttendanceReport(departmentId, period);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: api/Attendance/my?period=2025-08
        [HttpGet("my")]
        public async Task<IActionResult> GetMyAttendance([FromQuery] string period)
        {
            int currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (currentUserId == 0)
                return Unauthorized();

            try
            {
                var result = await _attendanceService.GetMyAttendance(currentUserId, period);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}