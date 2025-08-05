using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using QuestPDF.Fluent;
using ShiftManagement.Data;
using ShiftManagement.DTOs;

namespace ShiftManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Director,TeamLeader")]
    public class ExportController : ControllerBase
    {
        private readonly ShiftManagementContext _context;

        public ExportController(ShiftManagementContext context)
        {
            _context = context;
        }

        [HttpGet("attendance")]
        public async Task<IActionResult> ExportAttendance(
            [FromQuery] int departmentId,
            [FromQuery] string period,
            [FromQuery] string format = "excel")
        {
            if (!DateTime.TryParse(period + "-01", out DateTime monthStart))
                return BadRequest("Invalid period format. Use yyyy-MM");

            var monthEnd = monthStart.AddMonths(1);

            var data = await _context.ShiftSchedules
                .Where(s => s.DepartmentID == departmentId && s.Date >= monthStart && s.Date < monthEnd)
                .Select(s => new
                {
                    s.Employee.UserID,
                    s.Employee.FullName,
                    Details = s.ShiftScheduleDetails.Select(d => d.WorkUnit)
                })
                .AsNoTracking()
                .ToListAsync();

            var dtoData = data
                .GroupBy(s => new { s.UserID, s.FullName })
                .Select(g => new AttendanceExportDto
                {
                    EmployeeID = g.Key.UserID,
                    Name = g.Key.FullName,
                    TotalWorkUnit = g.SelectMany(x => x.Details).Sum()
                }).ToList();

            if (format.ToLower() == "excel")
            {
                var stream = new MemoryStream();
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage(stream))
                {
                    var sheet = package.Workbook.Worksheets.Add("Attendance");
                    sheet.Cells[1, 1].Value = "Employee ID";
                    sheet.Cells[1, 2].Value = "Name";
                    sheet.Cells[1, 3].Value = "Total Work Units";

                    int row = 2;
                    foreach (var item in dtoData)
                    {
                        sheet.Cells[row, 1].Value = item.EmployeeID;
                        sheet.Cells[row, 2].Value = item.Name;
                        sheet.Cells[row, 3].Value = item.TotalWorkUnit;
                        row++;
                    }

                    package.Save();
                }
                stream.Position = 0;
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "AttendanceReport.xlsx");
            }
            else if (format.ToLower() == "pdf")
            {
                var pdf = QuestPDF.Fluent.Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Margin(20);
                        page.Header().Text("Attendance Report").FontSize(18).Bold().AlignCenter();
                        page.Content().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.ConstantColumn(80);
                                c.RelativeColumn(2);
                                c.ConstantColumn(100);
                            });

                            table.Header(h =>
                            {
                                h.Cell().Text("Employee ID").Bold();
                                h.Cell().Text("Name").Bold();
                                h.Cell().Text("Total Units").Bold();
                            });

                            foreach (var item in dtoData)
                            {
                                table.Cell().Text(item.EmployeeID.ToString());
                                table.Cell().Text(item.Name);
                                table.Cell().Text(item.TotalWorkUnit.ToString());
                            }
                        });
                    });
                }).GeneratePdf();

                return File(pdf, "application/pdf", "AttendanceReport.pdf");
            }

            return BadRequest("Invalid format. Use 'excel' or 'pdf'.");
        }
    }
}