namespace ShiftManagement.DTOs.Sams
{
    public class TeamTemplateBindingDto
    {
        public int TeamTemplateBindingID { get; set; }
        public int TeamID { get; set; }
        public int TemplateID { get; set; }
        public string Target { get; set; } = "Roster";
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
    }
}