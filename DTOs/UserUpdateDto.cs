using System.ComponentModel.DataAnnotations;

namespace ShiftManagement.DTOs
{
    public class UserUpdateDto
    {
        [MaxLength(100)]
        public string FullName { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [MaxLength(20)]
        public string PhoneNumber { get; set; }

        public int? DepartmentID { get; set; }
        public int? StoreID { get; set; }

        public bool Status { get; set; }
    }
}
