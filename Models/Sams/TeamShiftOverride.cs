using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShiftManagement.Models.Sams
{
    [Table("TeamShiftOverride", Schema = "sams")]
    public class TeamShiftOverride
    {
        [Key]
        public int TeamShiftOverrideID { get; set; }

        [Required]
        public int TeamID { get; set; }

        [Required]
        public int ShiftBaseID { get; set; }

        [Column(TypeName = "time(0)")]
        public TimeSpan? StartTime { get; set; }

        [Column(TypeName = "time(0)")]
        public TimeSpan? EndTime { get; set; }

        public int? BreakMinutes { get; set; }

        public bool? IsOvernight { get; set; }

        [MaxLength(200)]
        public string? Note { get; set; }

        public Team? Team { get; set; }
        public ShiftBase? ShiftBase { get; set; }
    }
}