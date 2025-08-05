using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShiftManagement.Models
{
    public class UserRole
    {
        [Key]
        public int UserRoleID { get; set; }
        public int UserID { get; set; }
        public int RoleID { get; set; }

        public User? User { get; set; }
        public Role? Role { get; set; }
    }
}
