namespace ShiftManagement.DTOs.Sams
{
    public class TeamSettingsDto
    {
        public int TeamID { get; set; }
        public string SettingsJson { get; set; } = "{}";
        public DateTime UpdatedAt { get; set; }
    }
}