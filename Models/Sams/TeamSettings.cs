using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShiftManagement.Models.Sams
{
    [Table("TeamSettings", Schema = "sams")]
    public class TeamSettings
    {
        [Key]
        [ForeignKey(nameof(Team))]
        public int TeamID { get; set; }

        [Required]
        public string SettingsJson { get; set; } = "{}";

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Team? Team { get; set; }
    }
}