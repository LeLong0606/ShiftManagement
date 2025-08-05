using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShiftManagement.Models
{
    public class ShiftHistory
    {   
        [Key]
        public int HistoryID { get; set; }
        public int ScheduleID { get; set; }
        public int? ChangedBy { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public DateTime ChangeDate { get; set; }

        public ShiftSchedule? ShiftSchedule { get; set; }
        public User? ChangedUser { get; set; }
    }
}
