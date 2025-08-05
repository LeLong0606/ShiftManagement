using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ShiftManagement.Models
{
    public class Store
    {
        [Key]
        public int StoreID { get; set; }

        [Required]
        [MaxLength(100)]
        public string StoreName { get; set; }

        [MaxLength(200)]
        public string Address { get; set; }

        [MaxLength(20)]
        public string Phone { get; set; }

        public ICollection<Department> Departments { get; set; }
        public ICollection<User> Users { get; set; }
        public ICollection<ShiftSchedule> ShiftSchedules { get; set; }
    }
}
