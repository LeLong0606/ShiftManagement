namespace ShiftManagement.DTOs.Sams
{
    public class TeamShiftAliasDto
    {
        public int TeamShiftAliasID { get; set; }
        public int TeamID { get; set; }
        public int? ShiftBaseID { get; set; }
        public string? LeaveCode { get; set; }
        public string AliasCode { get; set; } = string.Empty;
    }
}