namespace ShiftManagement.DTOs.Sams
{
    public class VRosterForExportDto
    {
        public long RosterEntryID { get; set; }
        public int TeamID { get; set; }
        public int EmployeeID { get; set; }
        public string EmpCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime WorkDate { get; set; }
        public string CodeDisplay { get; set; } = string.Empty;
        public DateTime? PlanStart { get; set; }
        public DateTime? PlanEnd { get; set; }
    }
}