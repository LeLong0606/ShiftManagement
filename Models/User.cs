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
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public int? DepartmentID { get; set; }
        public int? StoreID { get; set; }
        public bool Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Department? Department { get; set; }
        public Store? Store { get; set; }
        public ICollection<UserRole>? UserRoles { get; set; }
        public ICollection<Log>? Logs { get; set; }
        public ICollection<ShiftSchedule>? ShiftSchedules { get; set; }
        public ICollection<ShiftSchedule>? CreatedSchedules { get; set; }
        public ICollection<ShiftHistory>? ShiftHistories { get; set; }
    }
}
