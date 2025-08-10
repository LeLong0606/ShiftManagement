using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShiftManagement.Models.Sams
{
    [Table("Employee", Schema = "sams")]
    public class Employee
    {
        [Key]
        public int EmployeeID { get; set; }

        [Required, MaxLength(50)]
        public string EmpCode { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string FullName { get; set; } = string.Empty;

        public int? TeamID { get; set; }

        public int? PositionID { get; set; }

        [MaxLength(30)]
        public string Status { get; set; } = "Active";

        [MaxLength(30)]
        public string? Phone { get; set; }

        [Column(TypeName = "date")]
        public DateTime? HireDate { get; set; }

        // JSON flexible attributes per team
        public string? Attributes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Team? Team { get; set; }
        public Position? Position { get; set; }

        public ICollection<RosterEntry>? RosterEntries { get; set; }
        public ICollection<TimesheetEntry>? TimesheetEntries { get; set; }
    }
}