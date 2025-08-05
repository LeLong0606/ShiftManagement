// DTOs/LogCreateDto.cs
using System.ComponentModel.DataAnnotations;

namespace ShiftManagement.DTOs
{
    public class LogCreateDto
    {
        [Required]
        public int UserID { get; set; }    // <-- non-nullable

        [Required]
        public string Action { get; set; } = default!;
    }
}