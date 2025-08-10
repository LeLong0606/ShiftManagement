namespace ShiftManagement.DTOs.Sams
{
    public class ShiftBaseDto
    {
        public int ShiftBaseID { get; set; }
        public string Code { get; set; } = string.Empty;     // C1/C2/OFF/P
        public string? Name { get; set; }
        public string? StartTime { get; set; }               // "HH:mm"
        public string? EndTime { get; set; }                 // "HH:mm"
        public int BreakMinutes { get; set; }
        public bool IsOvernight { get; set; }
        public string? Category { get; set; }                // WORK|OFF|LEAVE
    }
}