using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShiftManagement.Models
{
    public class Holiday
    {
        [Key]
        public int HolidayID { get; set; }
        public DateTime Date { get; set; }
        public string? Description { get; set; }
        public int? DefaultShiftCodeID { get; set; }

        public ShiftCode? DefaultShiftCode { get; set; }
    }
}
