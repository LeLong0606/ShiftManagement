using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShiftManagement.Models
{
    public class ShiftSchedule
    {
        [Key]
        public int ScheduleID { get; set; }
        public int EmployeeID { get; set; }
        public int DepartmentID { get; set; }
        public int StoreID { get; set; }
        public DateTime Date { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public User? Employee { get; set; }
        public Department? Department { get; set; }
        public Store? Store { get; set; }
        public User? CreatedUser { get; set; }
        public ICollection<ShiftScheduleDetail>? ShiftScheduleDetails { get; set; }
        public ICollection<ShiftHistory>? ShiftHistories { get; set; }
    }
}
