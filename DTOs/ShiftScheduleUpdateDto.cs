// ShiftManagement.DTOs/ShiftScheduleUpdateDto.cs
// nếu logic update giống create, bạn có thể reuse CreateDto
using System.ComponentModel.DataAnnotations;

namespace ShiftManagement.DTOs
{
    public class ShiftScheduleUpdateDto : ShiftScheduleCreateDto
    {
        // thừa kế hết, bạn chỉ cần nhận thêm ScheduleID nếu muốn
        [Required] public int ScheduleID { get; set; }
    }
}