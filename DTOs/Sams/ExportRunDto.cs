namespace ShiftManagement.DTOs.Sams
{
    public class ExportRunDto
    {
        public long ExportRunID { get; set; }
        public int TeamID { get; set; }
        public string Target { get; set; } = "Roster";
        public DateTime? PeriodStart { get; set; }
        public DateTime? PeriodEnd { get; set; }
        public int? TemplateID { get; set; }
        public string? FilePath { get; set; }
        public string Status { get; set; } = "Completed";
        public DateTime CreatedAt { get; set; }
    }
}