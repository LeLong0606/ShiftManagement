using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShiftManagement.Models.Sams
{
    [Table("TeamShiftAlias", Schema = "sams")]
    public class TeamShiftAlias
    {
        [Key]
        public int TeamShiftAliasID { get; set; }

        [Required]
        public int TeamID { get; set; }

        public int? ShiftBaseID { get; set; } // null for leave-only alias

        [MaxLength(20)]
        public string? LeaveCode { get; set; }

        [Required, MaxLength(20)]
        public string AliasCode { get; set; } = string.Empty;

        public Team? Team { get; set; }
        public ShiftBase? ShiftBase { get; set; }
    }
}