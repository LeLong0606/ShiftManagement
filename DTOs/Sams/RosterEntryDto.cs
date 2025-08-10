namespace ShiftManagement.DTOs.Sams
{
    // DTO dùng cho CRUD phân ca (dữ liệu thô)
    public class RosterEntryDto
    {
        public long RosterEntryID { get; set; }
        public int RosterPeriodID { get; set; }
        public int EmployeeID { get; set; }
        public DateTime WorkDate { get; set; }
        public int? ShiftBaseID { get; set; }    // null => nghỉ/leave
        public string? LeaveCode { get; set; }   // OFF/P...
        public string? StartTimeOverride { get; set; } // "HH:mm"
        public string? EndTimeOverride { get; set; }   // "HH:mm"
        public string? Note { get; set; }
        public string? Attributes { get; set; } // JSON
    }
}