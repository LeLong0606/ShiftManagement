using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShiftManagement.Models.Sams
{
    [Table("ExportRun", Schema = "sams")]
    public class ExportRun
    {
        [Key]
        public long ExportRunID { get; set; }

        [Required]
        public int TeamID { get; set; }

        [Required, MaxLength(20)]
        public string Target { get; set; } = "Roster"; // Roster | Timesheet

        [Column(TypeName = "date")]
        public DateTime? PeriodStart { get; set; }

        [Column(TypeName = "date")]
        public DateTime? PeriodEnd { get; set; }

        public int? TemplateID { get; set; }

        [MaxLength(500)]
        public string? FilePath { get; set; }

        [Required, MaxLength(20)]
        public string Status { get; set; } = "Completed";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Team? Team { get; set; }
        public ExportTemplate? Template { get; set; }
    }
}