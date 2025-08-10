using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static QuestPDF.Helpers.Colors;

namespace ShiftManagement.Models.Sams
{
    [Table("Department", Schema = "sams")]
    public class Department
    {
        [Key]
        public int DepartmentID { get; set; }

        [Required]
        public int LocationID { get; set; }

        [Required, MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public Location? Location { get; set; }
        public ICollection<Team>? Teams { get; set; }
    }
}