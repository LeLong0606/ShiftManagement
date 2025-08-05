// DTOs/LogDto.cs
namespace ShiftManagement.DTOs
{
    /// <summary>
    /// DTO dùng trả về thông tin log cho client. Tái sử dụng cho nhiều API.
    /// </summary>
    public class LogDto
    {
        public int LogID { get; set; }
        public int UserID { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime Timestamp { get; set; }
    }
}