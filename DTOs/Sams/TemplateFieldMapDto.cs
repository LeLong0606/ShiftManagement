namespace ShiftManagement.DTOs.Sams
{
    public class TemplateFieldMapDto
    {
        public int TemplateFieldMapID { get; set; }
        public int TemplateID { get; set; }
        public string Placeholder { get; set; } = string.Empty;
        public string SourceType { get; set; } = "Column";
        public string SourceExpr { get; set; } = string.Empty;
        public string? Format { get; set; }
    }
}