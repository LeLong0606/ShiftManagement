using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShiftManagement.Models.Sams
{
    [Table("RosterEntry", Schema = "sams")]
    public class RosterEntry
    {
        [Key]
        public long RosterEntryID { get; set; }

        [Required]
        public int RosterPeriodID { get; set; }

        [Required]
        public int EmployeeID { get; set; }

        [Column(TypeName = "date")]
        public DateTime WorkDate { get; set; }

        public int? ShiftBaseID { get; set; } // null -> leave/off day

        [MaxLength(20)]
        public string? LeaveCode { get; set; } // OFF/P...

        [Column(TypeName = "time(0)")]
        public TimeSpan? StartTimeOverride { get; set; }

        [Column(TypeName = "time(0)")]
        public TimeSpan? EndTimeOverride { get; set; }

        [MaxLength(200)]
        public string? Note { get; set; }

        public string? Attributes { get; set; } // JSON

        public RosterPeriod? RosterPeriod { get; set; }
        public Employee? Employee { get; set; }
        public ShiftBase? ShiftBase { get; set; }
    }
}