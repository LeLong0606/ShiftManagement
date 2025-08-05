// ShiftManagement.DTOs/ShiftScheduleDetailDto.cs
namespace ShiftManagement.DTOs
{
    public class ShiftScheduleDetailDto
    {
        public int DetailID { get; set; }
        public int ShiftCodeID { get; set; }
        public string ShiftCode { get; set; } = default!;  // mã ca như "X", "PN",…
        public string? ShiftType { get; set; }             // ví dụ "Morning"/"Afternoon"
        public int WorkUnit { get; set; }                  // số công tương ứng
    }
}