// ShiftManagement.DTOs/UserRoleDto.cs
namespace ShiftManagement.DTOs
{
    /// <summary>
    /// DTO trả về thông tin user-role cho client
    /// </summary>
    public class UserRoleDto
    {
        public int UserID { get; set; }
        public string Username { get; set; } = string.Empty;
        public int RoleID { get; set; }
        public string RoleName { get; set; } = string.Empty;
    }
}
