// ShiftManagement.DTOs/ShiftScheduleCreateDto.cs
using System.ComponentModel.DataAnnotations;

namespace ShiftManagement.DTOs
{
    public class ShiftScheduleCreateDto
    {
        [Required] public int EmployeeID { get; set; }
        [Required] public int DepartmentID { get; set; }
        [Required] public int StoreID { get; set; }
        [Required] public DateTime Date { get; set; }

        [Required]
        public List<ShiftScheduleDetailCreateDto> Details { get; set; } = new();
    }
}
