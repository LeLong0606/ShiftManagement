namespace ShiftManagement.DTOs
{
    /// <summary>
    /// DTO dùng để trả về thông tin người dùng cho client. Có thể tái sử dụng ở nhiều API.
    /// </summary>
    public class UserDto
    {
        public int UserID { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public int? DepartmentID { get; set; }
        public string? DepartmentName { get; set; }
        public int? StoreID { get; set; }
        public string? StoreName { get; set; }
        public bool Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
