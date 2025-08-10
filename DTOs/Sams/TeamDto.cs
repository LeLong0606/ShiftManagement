namespace ShiftManagement.DTOs.Sams
{
    public class TeamDto
    {
        public int TeamID { get; set; }
        public int DepartmentID { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}