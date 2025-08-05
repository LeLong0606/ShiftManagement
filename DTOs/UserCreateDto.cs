using System.ComponentModel.DataAnnotations;

namespace ShiftManagement.DTOs
{
    public class UserCreateDto
    {
        [Required, MaxLength(50)]
        public string Username { get; set; }

        [Required, MinLength(6)]
        public string Password { get; set; }

        [MaxLength(100)]
        public string FullName { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [MaxLength(20)]
        public string PhoneNumber { get; set; }

        public int? DepartmentID { get; set; }
        public int? StoreID { get; set; }
    }
}
