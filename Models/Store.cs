using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ShiftManagement.Models
{
    public class Store
    {
        [Key]
        public int StoreID { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Phone { get; set; }

        public ICollection<Department>? Departments { get; set; }
        public ICollection<User>? Users { get; set; }
        public ICollection<ShiftSchedule>? ShiftSchedules { get; set; }
    }
}
