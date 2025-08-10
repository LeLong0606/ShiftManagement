namespace ShiftManagement.DTOs.Sams
{
    public class TimesheetBatchDto
    {
        public long TimesheetBatchID { get; set; }
        public int TeamID { get; set; }
        public DateTime PeriodStartDate { get; set; }
        public DateTime PeriodEndDate { get; set; }
        public int? SourceRosterID { get; set; }
        public string Status { get; set; } = "Generated";
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}