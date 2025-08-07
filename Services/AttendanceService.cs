using Microsoft.EntityFrameworkCore;
using ShiftManagement.Data;
using ShiftManagement.DTOs;
using System.Security.Claims;

namespace ShiftManagement.Services
{
    public class AttendanceService
    {
        private readonly ShiftManagementContext _context;

        public AttendanceService(ShiftManagementContext context)
        {
            _context = context;
        }

        public async Task<List<AttendanceReportDto>> GetAttendanceReport(int departmentId, string period)
        {
            if (!DateTime.TryParse(period + "-01", out DateTime monthStart))
                throw new ArgumentException("Invalid period format. Use yyyy-MM");

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

            return dtoReport;
        }

        public async Task<List<MyAttendanceDto>> GetMyAttendance(int userId, string period)
        {
            if (!DateTime.TryParse(period + "-01", out DateTime monthStart))
                throw new ArgumentException("Invalid period format. Use yyyy-MM");

            var monthEnd = monthStart.AddMonths(1);

            var myReport = await _context.ShiftSchedules
                .AsNoTracking()
                .Where(s => s.EmployeeID == userId && s.Date >= monthStart && s.Date < monthEnd)
                .Select(s => new MyAttendanceDto
                {
                    Date = s.Date,
                    TotalWorkUnit = s.ShiftScheduleDetails.Sum(d => d.WorkUnit)
                })
                .ToListAsync();

            return myReport;
        }
    }
}