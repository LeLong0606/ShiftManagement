namespace ShiftManagement.DTOs
{
    /// <summary>
    /// DTO dùng trả về thông tin role.
    /// Có thể tái sử dụng cho các API lấy role.
    /// </summary>
    public class RoleDto
    {
        public int RoleID { get; set; }
        public string RoleName { get; set; } = string.Empty;
        // Mở rộng thêm nếu cần các thông tin khác về role
    }
}
