using System.ComponentModel.DataAnnotations;

namespace ShiftManagement.DTOs
{
    /// <summary>
    /// DTO dùng cho API đổi mật khẩu.
    /// </summary>
    public class ChangePasswordDto
    {
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
