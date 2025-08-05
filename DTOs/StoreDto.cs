// ShiftManagement.DTOs/StoreDto.cs
namespace ShiftManagement.DTOs
{
    public class StoreDto
    {
        public int StoreID { get; set; }
        public string StoreName { get; set; } = default!;
        public string? Address { get; set; }
        public string? Phone { get; set; }

        // Chỉ map danh sách department cơ bản
        public List<DepartmentDto> Departments { get; set; } = new();
    }
}