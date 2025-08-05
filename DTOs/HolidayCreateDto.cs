// DTOs/HolidayCreateDto.cs
namespace ShiftManagement.DTOs
{
    public class HolidayCreateDto
    {
        public DateTime Date { get; set; }
        public int DefaultShiftCodeID { get; set; }
    }
}