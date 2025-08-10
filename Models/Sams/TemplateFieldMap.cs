using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShiftManagement.Models.Sams
{
    [Table("TemplateFieldMap", Schema = "sams")]
    public class TemplateFieldMap
    {
        [Key]
        public int TemplateFieldMapID { get; set; }

        [Required]
        public int TemplateID { get; set; }

        [Required, MaxLength(100)]
        public string Placeholder { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string SourceType { get; set; } = "Column"; // Column | SQL | JsonPath | Pivot

        [Required]
        public string SourceExpr { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Format { get; set; }

        public ExportTemplate? Template { get; set; }
    }
}