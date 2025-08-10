namespace ShiftManagement.DTOs.Sams
{
    public class ExportTemplateDto
    {
        public int TemplateID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Target { get; set; } = "Roster"; // Roster|Timesheet
        public string Engine { get; set; } = "xlsx";
        public string LayoutJson { get; set; } = "{}";
        public int Version { get; set; } = 1;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
    }
}