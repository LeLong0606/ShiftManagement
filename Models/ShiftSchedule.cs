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

        [ForeignKey("Users")]
        public int EmployeeID { get; set; }
        public User Employee { get; set; }

        [ForeignKey("Departments")]
        public int DepartmentID { get; set; }
        public Department Department { get; set; }

        [ForeignKey("Stores")]
        public int StoreID { get; set; }
        public Store Store { get; set; }

        public DateTime Date { get; set; }

        [ForeignKey("Users")]
        public int? CreatedBy { get; set; }
        public User CreatedUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public ICollection<ShiftScheduleDetail> ShiftScheduleDetails { get; set; }
        public ICollection<ShiftHistory> ShiftHistories { get; set; }
    }
}
