namespace ShiftManagement.DTOs
{
    /// <summary>
    /// DTO dùng cho API đăng nhập lấy JWT token.
    /// </summary>
    public class LoginDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
