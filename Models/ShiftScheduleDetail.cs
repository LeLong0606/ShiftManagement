using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShiftManagement.Models
{
    public class ShiftScheduleDetail
    {
        [Key]
        public int DetailID { get; set; }

        [ForeignKey("ShiftSchedule")]
        public int ScheduleID { get; set; }
        public ShiftSchedule Schedule { get; set; }

        [ForeignKey("ShiftCodes")]
        public int ShiftCodeID { get; set; }
        public ShiftCode ShiftCode { get; set; }

        [MaxLength(20)]
        public string ShiftType { get; set; } = "Morning";

        public decimal WorkUnit { get; set; } = 1;
    }
}
