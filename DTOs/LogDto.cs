// DTOs/LogDto.cs
namespace ShiftManagement.DTOs
{
    public class LogDto
    {
        public int LogID { get; set; }
        public int UserID { get; set; }
        public string Username { get; set; } = default!;
        public string Action { get; set; } = default!;
        public DateTime Timestamp { get; set; }
    }
}