namespace ShiftManagement.DTOs.Sams
{
    public class TimesheetEntryDto
    {
        public long TimesheetEntryID { get; set; }
        public long TimesheetBatchID { get; set; }
        public int EmployeeID { get; set; }
        public DateTime WorkDate { get; set; }
        public string Kind { get; set; } = "WORK"; // WORK|OFF|LEAVE
        public string? CodeDisplay { get; set; }
        public DateTime? PlanStart { get; set; }
        public DateTime? PlanEnd { get; set; }
        public int? PlannedMinutes { get; set; }
        public string? Note { get; set; }
        public string? Attributes { get; set; }
    }
}