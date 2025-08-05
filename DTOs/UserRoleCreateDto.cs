namespace ShiftManagement.DTOs
{
    /// <summary>
    /// DTO dùng để nhận thông tin khi tạo/gán role cho user.
    /// Tái sử dụng cho API tạo UserRole.
    /// </summary>
    public class UserRoleCreateDto
    {
        public int UserID { get; set; }
        public int RoleID { get; set; }
    }
}
