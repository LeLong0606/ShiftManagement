// ShiftManagement.DTOs/UserRoleDto.cs
namespace ShiftManagement.DTOs
{
    /// <summary>
    /// DTO dùng trả về thông tin user-role (mối quan hệ user và role).
    /// Tái sử dụng cho các API liên quan phân quyền.
    /// </summary>
    public class UserRoleDto
    {
        public int UserID { get; set; }
        public string Username { get; set; } = string.Empty;
        public int RoleID { get; set; }
        public string RoleName { get; set; } = string.Empty;
    }
}
