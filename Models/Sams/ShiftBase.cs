using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShiftManagement.Models.Sams
{
    [Table("ShiftBase", Schema = "sams")]
    public class ShiftBase
    {
        [Key]
        public int ShiftBaseID { get; set; }

        [Required, MaxLength(20)]
        public string Code { get; set; } = string.Empty; // C1, C2, OFF, P...

        [MaxLength(100)]
        public string? Name { get; set; }

        [Column(TypeName = "time(0)")]
        public TimeSpan? StartTime { get; set; }

        [Column(TypeName = "time(0)")]
        public TimeSpan? EndTime { get; set; }

        public int BreakMinutes { get; set; } = 0;

        public bool IsOvernight { get; set; } = false;

        [MaxLength(20)]
        public string? Category { get; set; } // WORK | OFF | LEAVE...

        public ICollection<TeamShiftOverride>? TeamOverrides { get; set; }
        public ICollection<TeamShiftAlias>? TeamAliases { get; set; }
        public ICollection<RosterEntry>? RosterEntries { get; set; }
    }
}