using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShiftManagement.Models.Sams
{
    [Table("TeamTemplateBinding", Schema = "sams")]
    public class TeamTemplateBinding
    {
        [Key]
        public int TeamTemplateBindingID { get; set; }

        [Required]
        public int TeamID { get; set; }

        [Required]
        public int TemplateID { get; set; }

        [Required, MaxLength(20)]
        public string Target { get; set; } = "Roster"; // Roster | Timesheet

        [Column(TypeName = "date")]
        public DateTime EffectiveFrom { get; set; }

        [Column(TypeName = "date")]
        public DateTime? EffectiveTo { get; set; }

        public Team? Team { get; set; }
        public ExportTemplate? Template { get; set; }
    }
}