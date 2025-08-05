// ShiftManagement.DTOs/ShiftScheduleDto.cs
namespace ShiftManagement.DTOs
{
    public class ShiftScheduleDto
    {
        public int ScheduleID { get; set; }
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; } = default!;
        public int DepartmentID { get; set; }
        public string DepartmentName { get; set; } = default!;
        public int StoreID { get; set; }
        public string StoreName { get; set; } = default!;
        public DateTime Date { get; set; }

        public int CreatedBy { get; set; }
        public string CreatedUser { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public List<ShiftScheduleDetailDto> Details { get; set; } = new();
    }
}