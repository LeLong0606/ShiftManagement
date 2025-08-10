using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShiftManagement.Models.Sams
{
    [Table("ExportTemplate", Schema = "sams")]
    public class ExportTemplate
    {
        [Key]
        public int TemplateID { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string Target { get; set; } = "Roster"; // Roster | Timesheet

        [Required, MaxLength(20)]
        public string Engine { get; set; } = "xlsx";

        [Required]
        public string LayoutJson { get; set; } = "{}";

        public int Version { get; set; } = 1;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<TemplateFieldMap>? FieldMaps { get; set; }
        public ICollection<TeamTemplateBinding>? TeamBindings { get; set; }
    }
}