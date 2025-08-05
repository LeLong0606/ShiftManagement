using System.ComponentModel.DataAnnotations;

namespace ShiftManagement.DTOs
{
    public class ChangePasswordDto
    {
        [Required]
        public string OldPassword { get; set; }

        [Required, MinLength(6)]
        public string NewPassword { get; set; }
    }
}
