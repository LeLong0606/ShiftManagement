// ShiftManagement.DTOs/ShiftScheduleDetailCreateDto.cs
using System.ComponentModel.DataAnnotations;

namespace ShiftManagement.DTOs
{
    public class ShiftScheduleDetailCreateDto
    {
        [Required] public int ShiftCodeID { get; set; }
        public string? ShiftType { get; set; }
        public int? WorkUnit { get; set; }
    }
}