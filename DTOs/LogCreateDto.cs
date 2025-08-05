// DTOs/LogCreateDto.cs
using System.ComponentModel.DataAnnotations;

namespace ShiftManagement.DTOs
{
    /// <summary>
    /// DTO dùng cho API tạo mới log.
    /// </summary>
    public class LogCreateDto
    {
        public int UserID { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? Timestamp { get; set; }
    }
}