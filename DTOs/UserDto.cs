namespace ShiftManagement.DTOs
{
    public class UserDto
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }

        public int? DepartmentID { get; set; }
        public string? DepartmentName { get; set; }

        public int? StoreID { get; set; }
        public string? StoreName { get; set; }

        public bool Status { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
