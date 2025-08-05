using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ShiftManagement.Models
{
    public class ShiftCode
    {
        [Key]
        public int ShiftCodeID { get; set; }
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal WorkUnit { get; set; }
        public bool IsLeave { get; set; }

        public ICollection<ShiftScheduleDetail>? ShiftScheduleDetails { get; set; }
        public ICollection<Holiday>? Holidays { get; set; }
        public ICollection<WorkUnitRule>? WorkUnitRules { get; set; }
    }
}
