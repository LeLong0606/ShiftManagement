// DTOs/HolidayDto.cs
namespace ShiftManagement.DTOs
{
    public class HolidayDto
    {
        public int HolidayID { get; set; }
        public DateTime Date { get; set; }
        public int DefaultShiftCodeID { get; set; }
        public string DefaultShiftCode { get; set; } = null!;
    }
}