namespace ShiftManagement.DTOs
{
    public class RoleWithUsersDto : RoleDto
    {
        public List<string> Usernames { get; set; } = new();
    }
}
