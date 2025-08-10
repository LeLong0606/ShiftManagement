namespace ShiftManagement.DTOs.Sams
{
    public class RosterPeriodDto
    {
        public int RosterPeriodID { get; set; }
        public int TeamID { get; set; }
        public DateTime PeriodStartDate { get; set; }
        public DateTime PeriodEndDate { get; set; }
        public string PeriodType { get; set; } = "Month"; // Month|Week
        public string Status { get; set; } = "Draft";     // Draft|Published|Locked
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}