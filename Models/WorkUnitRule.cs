using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShiftManagement.Models
{
    public class WorkUnitRule
    {
        [Key]
        public int RuleID { get; set; }
        public int ShiftCodeID { get; set; }
        public DateTime EffectiveDate { get; set; }
        public decimal WorkUnit { get; set; }

        public ShiftCode? ShiftCode { get; set; }
    }
}
