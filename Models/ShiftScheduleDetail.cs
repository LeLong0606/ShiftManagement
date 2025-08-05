using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShiftManagement.Models
{
    public class ShiftScheduleDetail
    {
        [Key]
        public int DetailID { get; set; }
        public int ScheduleID { get; set; }
        public int ShiftCodeID { get; set; }
        public string ShiftType { get; set; } = "Morning"; // default
        public decimal WorkUnit { get; set; }

        public ShiftSchedule? ShiftSchedule { get; set; }
        public ShiftCode? ShiftCode { get; set; }
    }
}
