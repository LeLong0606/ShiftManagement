namespace ShiftManagement.DTOs
{
    public class DepartmentDto
    {
        public int DepartmentID { get; set; }
        public string DepartmentName { get; set; }
        public int StoreID { get; set; }
        public string StoreName { get; set; }
        public int? ManagerID { get; set; }
    }
}