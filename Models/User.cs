using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShiftManagement.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; }

        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; }

        [MaxLength(100)]
        public string FullName { get; set; }

        [MaxLength(100)]
        public string Email { get; set; }

        [MaxLength(20)]
        public string PhoneNumber { get; set; }

        // 🔹 Quan hệ với Department
        public int? DepartmentID { get; set; }
        [ForeignKey("DepartmentID")]
        public Department Department { get; set; }

        // 🔹 Quan hệ với Store
        public int? StoreID { get; set; }
        [ForeignKey("StoreID")]
        public Store Store { get; set; }

        public bool Status { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // 🔹 Navigation Properties
        public ICollection<UserRole> UserRoles { get; set; }

        // Lịch mà user này là nhân viên (Employee)
        public ICollection<ShiftSchedule> ShiftSchedules { get; set; }

        // Lịch mà user này là người tạo (CreatedBy)
        public ICollection<ShiftSchedule> CreatedSchedules { get; set; }

        public ICollection<ShiftHistory> ShiftHistories { get; set; }
        public ICollection<Log> Logs { get; set; }
    }
}
