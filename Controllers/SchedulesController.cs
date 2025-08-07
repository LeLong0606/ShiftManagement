using Microsoft.AspNetCore.Mvc;
using ShiftManagement.DTOs;
using ShiftManagement.Services;
using Microsoft.AspNetCore.Authorization;

namespace ShiftManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SchedulesController : ControllerBase
    {
        private readonly ScheduleService _scheduleService;

        public SchedulesController(ScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        /// <summary>
        /// [GET] Lấy danh sách lịch làm việc, hỗ trợ lọc theo ngày, phòng ban. Có phân trang.
        /// Chỉ tài khoản đã đăng nhập mới truy cập được.
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ShiftScheduleDto>>> GetSchedules(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] int? departmentId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            var result = await _scheduleService.GetSchedulesAsync(from, to, departmentId, page, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// [POST] Tạo mới lịch làm việc. Chỉ Admin hoặc Manager được tạo.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ShiftScheduleDto>> PostSchedule([FromBody] ShiftScheduleCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdById = int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var value) ? value : 0;

            var result = await _scheduleService.CreateScheduleAsync(dto, createdById);

            if (result == null)
                return BadRequest("Tạo lịch làm việc thất bại.");

            return CreatedAtAction(nameof(GetSchedules),
                new { from = result.Date, to = result.Date, departmentId = result.DepartmentID },
                result);
        }

        /// <summary>
        /// [PUT] Cập nhật lịch làm việc. Chỉ Admin hoặc Manager được sửa.
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> PutSchedule(int id, [FromBody] ShiftScheduleUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _scheduleService.UpdateScheduleAsync(id, dto);

            if (!updated)
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// [DELETE] Xóa lịch làm việc theo ID. Chỉ Admin hoặc Manager được xóa.
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            var deleted = await _scheduleService.DeleteScheduleAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}