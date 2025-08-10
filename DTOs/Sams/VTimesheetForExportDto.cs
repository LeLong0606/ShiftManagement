namespace ShiftManagement.DTOs.Sams
{
    public class VTimesheetForExportDto
    {
        public long TimesheetEntryID { get; set; }
        public int TeamID { get; set; }
        public int EmployeeID { get; set; }
        public string EmpCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime WorkDate { get; set; }
        public string Kind { get; set; } = string.Empty;
        public string? CodeDisplay { get; set; }
        public DateTime? PlanStart { get; set; }
        public DateTime? PlanEnd { get; set; }
        public int? PlannedMinutes { get; set; }
    }
}