using Microsoft.AspNetCore.Mvc;
using ShiftManagement.DTOs;
using ShiftManagement.Services;
using Microsoft.AspNetCore.Authorization;

namespace ShiftManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        private readonly LogService _logService;

        public LogsController(LogService logService)
        {
            _logService = logService;
        }

        /// <summary>
        /// [GET] Lấy danh sách log, hỗ trợ tìm kiếm theo keyword, lọc theo User, phân trang.
        /// Chỉ tài khoản đã đăng nhập mới truy cập được.
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<LogDto>>> GetLogs(
            [FromQuery] string? search = "",
            [FromQuery] int? userId = null,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            var logs = await _logService.GetLogsAsync(search, userId, from, to, page, pageSize);
            return Ok(logs);
        }

        /// <summary>
        /// [GET] Lấy chi tiết một log theo ID. Chỉ tài khoản đã đăng nhập mới truy cập được.
        /// </summary>
        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<ActionResult<LogDto>> GetLog(int id)
        {
            var dto = await _logService.GetLogAsync(id);
            if (dto == null)
                return NotFound();
            return Ok(dto);
        }

        /// <summary>
        /// [POST] Tạo mới một log. Chỉ Admin hoặc hệ thống được tạo log thủ công.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<LogDto>> PostLog([FromBody] LogCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (resultDto, error) = await _logService.CreateLogAsync(dto);
            if (error != null)
                return NotFound(new { Message = error });

            return CreatedAtAction(nameof(GetLog), new { id = resultDto.LogID }, resultDto);
        }

        /// <summary>
        /// [DELETE] Xóa log theo ID. Chỉ Admin có quyền xóa.
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteLog(int id)
        {
            var deleted = await _logService.DeleteLogAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}