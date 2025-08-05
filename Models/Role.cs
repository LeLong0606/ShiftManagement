using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ShiftManagement.Models
{
    public class Role
    {
        [Key]
        public int RoleID { get; set; }
        public string RoleName { get; set; } = string.Empty;

        public ICollection<UserRole>? UserRoles { get; set; }
    }
}
