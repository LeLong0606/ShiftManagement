using System.ComponentModel.DataAnnotations;

namespace ShiftManagement.DTOs
{
    /// <summary>
    /// DTO nhận thông tin tạo mới người dùng.
    /// Dùng cho API tạo mới user.
    /// </summary>
    public class UserCreateDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public int? DepartmentID { get; set; }
        public int? StoreID { get; set; }
    }
}
