using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShiftManagement.Models.Sams
{
    [Table("Holiday", Schema = "sams")]
    public class Holiday
    {
        [Key]
        public int HolidayID { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;
    }
}