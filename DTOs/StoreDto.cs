// ShiftManagement.DTOs/StoreDto.cs
namespace ShiftManagement.DTOs
{
    /// <summary>
    /// DTO dùng trả về thông tin cửa hàng.
    /// Tái sử dụng cho các API cửa hàng.
    /// </summary>
    public class StoreDto
    {
        public int StoreID { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Phone { get; set; }
    }
}