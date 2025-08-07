using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftManagement.Services;

namespace ShiftManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Director,TeamLeader")]
    public class ExportController : ControllerBase
    {
        private readonly ExportService _exportService;

        public ExportController(ExportService exportService)
        {
            _exportService = exportService;
        }

        [HttpGet("attendance")]
        public async Task<IActionResult> ExportAttendance(
            [FromQuery] int departmentId,
            [FromQuery] string period,
            [FromQuery] string format = "excel")
        {
            var (dtoData, error) = await _exportService.GetAttendanceExportDataAsync(departmentId, period);
            if (error != null) return BadRequest(error);

            if (format.ToLower() == "excel")
            {
                var bytes = _exportService.ExportAttendanceToExcel(dtoData);
                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "AttendanceReport.xlsx");
            }
            else if (format.ToLower() == "pdf")
            {
                var bytes = _exportService.ExportAttendanceToPdf(dtoData);
                return File(bytes, "application/pdf", "AttendanceReport.pdf");
            }

            return BadRequest("Invalid format. Use 'excel' or 'pdf'.");
        }
    }
}