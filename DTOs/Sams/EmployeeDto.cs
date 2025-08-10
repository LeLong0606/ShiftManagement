namespace ShiftManagement.DTOs.Sams
{
    public class EmployeeDto
    {
        public int EmployeeID { get; set; }
        public string EmpCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int? TeamID { get; set; }
        public int? PositionID { get; set; }
        public string Status { get; set; } = "Active";
        public string? Phone { get; set; }
        public DateTime? HireDate { get; set; }
        public string? Attributes { get; set; } // JSON
    }
}