using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShiftManagement.Models.Sams
{
    [Table("CalendarDate", Schema = "sams")]
    public class CalendarDate
    {
        [Key]
        [Column(TypeName = "date")]
        public System.DateTime Date { get; set; }

        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public byte Weekday { get; set; } // 1..7
        public bool IsWeekend { get; set; }
        public bool IsHoliday { get; set; }
    }
}