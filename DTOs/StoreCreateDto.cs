// ShiftManagement.DTOs/StoreCreateDto.cs
using System.ComponentModel.DataAnnotations;

namespace ShiftManagement.DTOs
{
    public class StoreCreateDto
    {
        [Required, MaxLength(100)]
        public string StoreName { get; set; } = default!;

        [MaxLength(200)]
        public string? Address { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }
    }
}