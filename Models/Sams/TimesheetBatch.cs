using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShiftManagement.Models.Sams
{
    [Table("TimesheetBatch", Schema = "sams")]
    public class TimesheetBatch
    {
        [Key]
        public long TimesheetBatchID { get; set; }

        [Required]
        public int TeamID { get; set; }

        [Column(TypeName = "date")]
        public DateTime PeriodStartDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime PeriodEndDate { get; set; }

        public int? SourceRosterID { get; set; }

        [Required, MaxLength(20)]
        public string Status { get; set; } = "Generated"; // Generated/Adjusted/Locked

        [MaxLength(100)]
        public string? CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Team? Team { get; set; }
        public RosterPeriod? SourceRoster { get; set; }

        public ICollection<TimesheetEntry>? TimesheetEntries { get; set; }
    }
}