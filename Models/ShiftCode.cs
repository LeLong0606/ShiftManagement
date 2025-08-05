using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ShiftManagement.Models
{
    public class ShiftCode
    {
        [Key]
        public int ShiftCodeID { get; set; }

        [Required]
        [MaxLength(10)]
        public string Code { get; set; }

        [MaxLength(100)]
        public string Description { get; set; }

        public decimal WorkUnit { get; set; } = 1;
        public bool IsLeave { get; set; } = false;

        public ICollection<ShiftScheduleDetail> ShiftScheduleDetails { get; set; }
        public ICollection<WorkUnitRule> WorkUnitRules { get; set; }
    }
}
