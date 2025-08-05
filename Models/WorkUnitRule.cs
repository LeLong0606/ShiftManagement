using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShiftManagement.Models
{
    public class WorkUnitRule
    {
        [Key]
        public int RuleID { get; set; }

        [ForeignKey("ShiftCodes")]
        public int ShiftCodeID { get; set; }
        public ShiftCode ShiftCode { get; set; }

        public DateTime EffectiveDate { get; set; } = DateTime.Now;
        public decimal WorkUnit { get; set; }
    }
}
