using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftManagement.Data;
using ShiftManagement.DTOs;
using ShiftManagement.Models;
using Microsoft.AspNetCore.Authorization;

namespace ShiftManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SchedulesController : ControllerBase
    {
        private readonly ShiftManagementContext _context;

        public SchedulesController(ShiftManagementContext context)
        {
            _context = context;
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
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 50;

            var query = _context.ShiftSchedules
                .AsNoTracking()
                .Include(s => s.Employee)
                .Include(s => s.CreatedUser)
                .Include(s => s.Department)
                .Include(s => s.Store)
                .Include(s => s.ShiftScheduleDetails).ThenInclude(d => d.ShiftCode)
                .AsQueryable();

            if (from.HasValue)
                query = query.Where(s => s.Date >= from.Value);
            if (to.HasValue)
                query = query.Where(s => s.Date <= to.Value);
            if (departmentId.HasValue)
                query = query.Where(s => s.DepartmentID == departmentId.Value);

            var result = await query
                .OrderBy(s => s.Date)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new ShiftScheduleDto
                {
                    ScheduleID = s.ScheduleID,
                    EmployeeID = s.EmployeeID,
                    EmployeeName = s.Employee.FullName,
                    DepartmentID = s.DepartmentID,
                    DepartmentName = s.Department != null ? s.Department.DepartmentName : "",
                    StoreID = s.StoreID,
                    StoreName = s.Store != null ? s.Store.StoreName : "",
                    Date = s.Date,
                    CreatedBy = s.CreatedBy ?? 0,
                    CreatedUser = s.CreatedUser != null ? s.CreatedUser.Username : "",
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt,
                    Details = s.ShiftScheduleDetails != null
                        ? s.ShiftScheduleDetails.Select(d => new ShiftScheduleDetailDto
                        {
                            DetailID = d.DetailID,
                            ShiftCodeID = d.ShiftCodeID,
                            ShiftCode = d.ShiftCode != null ? d.ShiftCode.Code : "",
                            ShiftType = d.ShiftType,
                            WorkUnit = Convert.ToInt32(d.WorkUnit)
                        }).ToList()
                        : new List<ShiftScheduleDetailDto>()
                })
                .ToListAsync();

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

            var entity = new ShiftSchedule
            {
                EmployeeID = dto.EmployeeID,
                DepartmentID = dto.DepartmentID,
                StoreID = dto.StoreID,
                Date = dto.Date,
                CreatedBy = createdById,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ShiftScheduleDetails = dto.Details != null
                    ? dto.Details.Select(d => new ShiftScheduleDetail
                    {
                        ShiftCodeID = d.ShiftCodeID,
                        ShiftType = d.ShiftType,
                        WorkUnit = d.WorkUnit ?? 0
                    }).ToList()
                    : new List<ShiftScheduleDetail>()
            };

            _context.ShiftSchedules.Add(entity);
            await _context.SaveChangesAsync();

            // Load navigation để trả về DTO đầy đủ
            await _context.Entry(entity).Reference(s => s.Employee).LoadAsync();
            await _context.Entry(entity).Reference(s => s.Department).LoadAsync();
            await _context.Entry(entity).Reference(s => s.Store).LoadAsync();
            await _context.Entry(entity).Reference(s => s.CreatedUser).LoadAsync();
            await _context.Entry(entity).Collection(s => s.ShiftScheduleDetails).LoadAsync();
            foreach (var detail in entity.ShiftScheduleDetails)
                await _context.Entry(detail).Reference(d => d.ShiftCode).LoadAsync();

            var result = new ShiftScheduleDto
            {
                ScheduleID = entity.ScheduleID,
                EmployeeID = entity.EmployeeID,
                EmployeeName = entity.Employee?.FullName ?? "",
                DepartmentID = entity.DepartmentID,
                DepartmentName = entity.Department?.DepartmentName ?? "",
                StoreID = entity.StoreID,
                StoreName = entity.Store?.StoreName ?? "",
                Date = entity.Date,
                CreatedBy = entity.CreatedBy ?? 0,
                CreatedUser = entity.CreatedUser?.Username ?? "",
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                Details = entity.ShiftScheduleDetails != null
                    ? entity.ShiftScheduleDetails.Select(d => new ShiftScheduleDetailDto
                    {
                        DetailID = d.DetailID,
                        ShiftCodeID = d.ShiftCodeID,
                        ShiftCode = d.ShiftCode?.Code ?? "",
                        ShiftType = d.ShiftType,
                        WorkUnit = Convert.ToInt32(d.WorkUnit)
                    }).ToList()
                    : new List<ShiftScheduleDetailDto>()
            };

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

            var entity = await _context.ShiftSchedules
                .Include(s => s.ShiftScheduleDetails)
                .FirstOrDefaultAsync(s => s.ScheduleID == id);

            if (entity == null)
                return NotFound();

            // Cập nhật các trường chính
            entity.EmployeeID = dto.EmployeeID;
            entity.DepartmentID = dto.DepartmentID;
            entity.StoreID = dto.StoreID;
            entity.Date = dto.Date;
            entity.UpdatedAt = DateTime.UtcNow;

            // Xử lý cập nhật chi tiết ca làm việc
            entity.ShiftScheduleDetails.Clear();
            if (dto.Details != null)
            {
                foreach (var d in dto.Details)
                {
                    entity.ShiftScheduleDetails.Add(new ShiftScheduleDetail
                    {
                        ShiftCodeID = d.ShiftCodeID,
                        ShiftType = d.ShiftType,
                        WorkUnit = d.WorkUnit ?? 0
                    });
                }
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// [DELETE] Xóa lịch làm việc theo ID. Chỉ Admin hoặc Manager được xóa.
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            var entity = await _context.ShiftSchedules.FindAsync(id);
            if (entity == null)
                return NotFound();

            _context.ShiftSchedules.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}