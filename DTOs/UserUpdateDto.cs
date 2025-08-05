using System.ComponentModel.DataAnnotations;

namespace ShiftManagement.DTOs
{
    /// <summary>
    /// DTO cập nhật role của user (nâng cấp/sửa role)
    /// </summary>
    public class UserRoleUpdateDto
    {
        public int NewRoleID { get; set; }
    }
}
