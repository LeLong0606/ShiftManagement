namespace ShiftManagement.DTOs.Sams
{
    public class TeamShiftOverrideDto
    {
        public int TeamShiftOverrideID { get; set; }
        public int TeamID { get; set; }
        public int ShiftBaseID { get; set; }
        public string? StartTime { get; set; }   // "HH:mm"
        public string? EndTime { get; set; }     // "HH:mm"
        public int? BreakMinutes { get; set; }
        public bool? IsOvernight { get; set; }
        public string? Note { get; set; }
    }
}