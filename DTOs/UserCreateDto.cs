using System.ComponentModel.DataAnnotations;

namespace ShiftManagement.DTOs
{
    /// <summary>
    /// DTO nhận thông tin khi gán role cho user (tạo mới UserRole)
    /// </summary>
    public class UserRoleCreateDto
    {
        public int UserID { get; set; }
        public int RoleID { get; set; }
    }
}
