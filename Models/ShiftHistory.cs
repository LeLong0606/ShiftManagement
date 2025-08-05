using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShiftManagement.Models
{
    public class ShiftHistory
    {
        [Key]
        public int HistoryID { get; set; }

        [ForeignKey("ShiftSchedule")]
        public int ScheduleID { get; set; }
        public ShiftSchedule Schedule { get; set; }

        [ForeignKey("Users")]
        public int? ChangedBy { get; set; }
        public User User { get; set; }

        public string OldValue { get; set; }
        public string NewValue { get; set; }

        public DateTime ChangeDate { get; set; } = DateTime.Now;
    }
}
