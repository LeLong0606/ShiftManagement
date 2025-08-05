using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShiftManagement.Models
{
    public class Department
    {
        [Key]
        public int DepartmentID { get; set; }

        [Required]
        [MaxLength(100)]
        public string DepartmentName { get; set; }

        [ForeignKey("Stores")]
        public int StoreID { get; set; }
        public Store Store { get; set; }

        public int? ManagerID { get; set; }

        public ICollection<User> Users { get; set; }
        public ICollection<ShiftSchedule> ShiftSchedules { get; set; }
    }
}
