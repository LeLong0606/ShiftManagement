using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShiftManagement.Models.Sams
{
    [Table("Team", Schema = "sams")]
    public class Team
    {
        [Key]
        public int TeamID { get; set; }

        [Required]
        public int DepartmentID { get; set; }

        [Required, MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public Department? Department { get; set; }
        public TeamSettings? Settings { get; set; }
        public ICollection<Employee>? Employees { get; set; }
        public ICollection<TeamShiftAlias>? ShiftAliases { get; set; }
        public ICollection<TeamShiftOverride>? ShiftOverrides { get; set; }
        public ICollection<RosterPeriod>? RosterPeriods { get; set; }
        public ICollection<TimesheetBatch>? TimesheetBatches { get; set; }
        public ICollection<TeamTemplateBinding>? TemplateBindings { get; set; }
        public ICollection<ExportRun>? ExportRuns { get; set; }
    }
}