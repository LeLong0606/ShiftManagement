using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShiftManagement.Models.Sams
{
    [Table("TimesheetEntry", Schema = "sams")]
    public class TimesheetEntry
    {
        [Key]
        public long TimesheetEntryID { get; set; }

        [Required]
        public long TimesheetBatchID { get; set; }

        [Required]
        public int EmployeeID { get; set; }

        [Column(TypeName = "date")]
        public DateTime WorkDate { get; set; }

        [Required, MaxLength(10)]
        public string Kind { get; set; } = "WORK"; // WORK | OFF | LEAVE

        [MaxLength(20)]
        public string? CodeDisplay { get; set; }

        public DateTime? PlanStart { get; set; }
        public DateTime? PlanEnd { get; set; }
        public int? PlannedMinutes { get; set; }

        [MaxLength(200)]
        public string? Note { get; set; }

        public string? Attributes { get; set; } // JSON

        public TimesheetBatch? TimesheetBatch { get; set; }
        public Employee? Employee { get; set; }
    }
}