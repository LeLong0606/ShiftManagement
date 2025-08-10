using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShiftManagement.Models.Sams
{
    [Table("RosterPeriod", Schema = "sams")]
    public class RosterPeriod
    {
        [Key]
        public int RosterPeriodID { get; set; }

        [Required]
        public int TeamID { get; set; }

        [Column(TypeName = "date")]
        public DateTime PeriodStartDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime PeriodEndDate { get; set; }

        [Required, MaxLength(20)]
        public string PeriodType { get; set; } = "Month"; // Month | Week

        [Required, MaxLength(20)]
        public string Status { get; set; } = "Draft"; // Draft/Published/Locked

        [MaxLength(100)]
        public string? CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Team? Team { get; set; }
        public ICollection<RosterEntry>? RosterEntries { get; set; }
    }
}