// ShiftManagement.DTOs/DepartmentDto.cs
namespace ShiftManagement.DTOs
{
    /// <summary>
    /// DTO dùng trả về thông tin phòng ban.
    /// Tái sử dụng cho các API phòng ban.
    /// </summary>
    public class DepartmentDto
    {
        public int DepartmentID { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public int? StoreID { get; set; }
        public string? StoreName { get; set; }
    }
}