using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShiftManagement.Models
{
    public class Holiday
    {
        [Key]
        public int HolidayID { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [MaxLength(100)]
        public string Description { get; set; }

        public int? DefaultShiftCodeID { get; set; }
        [ForeignKey("DefaultShiftCodeID")]
        public ShiftCode DefaultShiftCode { get; set; }
    }
}
