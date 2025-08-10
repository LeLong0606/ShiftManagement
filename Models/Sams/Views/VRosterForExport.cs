using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ShiftManagement.Models.Sams.Views
{
    [Keyless]
    [Table("vRosterForExport", Schema = "sams")]
    public class VRosterForExport
    {
        public long RosterEntryID { get; set; }
        public int TeamID { get; set; }
        public int EmployeeID { get; set; }
        public string EmpCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime WorkDate { get; set; }
        public string CodeDisplay { get; set; } = string.Empty;
        public DateTime? PlanStart { get; set; }
        public DateTime? PlanEnd { get; set; }
    }
}