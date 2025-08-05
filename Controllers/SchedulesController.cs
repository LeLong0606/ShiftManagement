using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftManagement.Data;
using ShiftManagement.DTOs;
using ShiftManagement.Models;

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

        // GET: api/Schedules?from=2025-08-01&to=2025-08-31&departmentId=1
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ShiftScheduleDto>>> GetSchedules(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] int? departmentId)
        {
            var query = _context.ShiftSchedules
                .AsNoTracking()
                .Include(s => s.Employee)
                .Include(s => s.CreatedUser)
                .Include(s => s.Department)
                .Include(s => s.Store)
                .Include(s => s.ShiftScheduleDetails!)
                    .ThenInclude(d => d.ShiftCode)
                .AsQueryable();

            if (from.HasValue) query = query.Where(s => s.Date >= from.Value);
            if (to.HasValue) query = query.Where(s => s.Date <= to.Value);
            if (departmentId.HasValue) query = query.Where(s => s.DepartmentID == departmentId.Value);

            var list = await query
                .OrderBy(s => s.Date)
                .Select(s => new ShiftScheduleDto
                {
                    ScheduleID = s.ScheduleID,
                    EmployeeID = s.EmployeeID,
                    EmployeeName = s.Employee.FullName!,
                    DepartmentID = s.DepartmentID,
                    DepartmentName = s.Department!.DepartmentName,
                    StoreID = s.StoreID,
                    StoreName = s.Store!.StoreName,
                    Date = s.Date,
                    CreatedBy = s.CreatedBy ?? 0,                       // ① int? → int
                    CreatedUser = s.CreatedUser.Username,                // ② User → string
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt,
                    Details = s.ShiftScheduleDetails!
                        .Select(d => new ShiftScheduleDetailDto
                        {
                            DetailID = d.DetailID,
                            ShiftCodeID = d.ShiftCodeID,
                            ShiftCode = d.ShiftCode!.Code,
                            ShiftType = d.ShiftType,
                            WorkUnit = (int)d.WorkUnit                   // ③ decimal → int
                        })
                        .ToList()
                })
                .ToListAsync();

            return Ok(list);
        }

        // POST: api/Schedules
        [HttpPost]
        public async Task<ActionResult<ShiftScheduleDto>> PostSchedule(
            [FromBody] ShiftScheduleCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Lấy userID từ JWT claims (nếu có)
            var createdById = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            var entity = new ShiftSchedule
            {
                EmployeeID = dto.EmployeeID,
                DepartmentID = dto.DepartmentID,
                StoreID = dto.StoreID,
                Date = dto.Date,
                CreatedBy = createdById,  // ⑤ gán vào int? CreatedBy
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ShiftScheduleDetails = dto.Details
                    .Select(d => new ShiftScheduleDetail
                    {
                        ShiftCodeID = d.ShiftCodeID,
                        ShiftType = d.ShiftType,
                        WorkUnit = d.WorkUnit ?? 0   // ⑧ nullable int → int
                    })
                    .ToList()
            };

            _context.ShiftSchedules.Add(entity);
            await _context.SaveChangesAsync();

            // Load navigation để mapping DTO
            await _context.Entry(entity).Reference(s => s.Employee).LoadAsync();
            await _context.Entry(entity).Reference(s => s.Department).LoadAsync();
            await _context.Entry(entity).Reference(s => s.Store).LoadAsync();
            await _context.Entry(entity).Reference(s => s.CreatedUser).LoadAsync();
            await _context.Entry(entity).Collection(s => s.ShiftScheduleDetails).LoadAsync();
            foreach (var detail in entity.ShiftScheduleDetails!)
                await _context.Entry(detail).Reference(d => d.ShiftCode).LoadAsync();

            var result = new ShiftScheduleDto
            {
                ScheduleID = entity.ScheduleID,
                EmployeeID = entity.EmployeeID,
                EmployeeName = entity.Employee.FullName!,
                DepartmentID = entity.DepartmentID,
                DepartmentName = entity.Department!.DepartmentName,
                StoreID = entity.StoreID,
                StoreName = entity.Store!.StoreName,
                Date = entity.Date,
                CreatedBy = entity.CreatedBy ?? 0,             // ⑥ int? → int
                CreatedUser = entity.CreatedUser.Username,      // ⑦ User → string
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                Details = entity.ShiftScheduleDetails!
                    .Select(d => new ShiftScheduleDetailDto
                    {
                        DetailID = d.DetailID,
                        ShiftCodeID = d.ShiftCodeID,
                        ShiftCode = d.ShiftCode!.Code,            // ⑦ mapping mã ca
                        ShiftType = d.ShiftType,
                        WorkUnit = (int)d.WorkUnit               // ⑧ decimal → int
                    })
                    .ToList()
            };

            return CreatedAtAction(
                nameof(GetSchedules),
                new { from = result.Date, to = result.Date, departmentId = result.DepartmentID },
                result
            );
        }

        // PUT: api/Schedules/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutSchedule(
            int id,
            [FromBody] ShiftScheduleUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (id != dto.ScheduleID)
                return BadRequest("ID mismatch");

            var existing = await _context.ShiftSchedules
                .Include(s => s.ShiftScheduleDetails!)
                .FirstOrDefaultAsync(s => s.ScheduleID == id);
            if (existing == null)
                return NotFound();

            existing.EmployeeID = dto.EmployeeID;
            existing.DepartmentID = dto.DepartmentID;
            existing.StoreID = dto.StoreID;
            existing.Date = dto.Date;
            existing.UpdatedAt = DateTime.UtcNow;

            // Thay thế chi tiết cũ bằng chi tiết mới
            _context.ShiftScheduleDetails.RemoveRange(existing.ShiftScheduleDetails!);
            existing.ShiftScheduleDetails = dto.Details
                .Select(d => new ShiftScheduleDetail
                {
                    ShiftCodeID = d.ShiftCodeID,
                    ShiftType = d.ShiftType,
                    WorkUnit = d.WorkUnit ?? 0
                })
                .ToList();

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Schedules/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            var existing = await _context.ShiftSchedules.FindAsync(id);
            if (existing == null)
                return NotFound();

            _context.ShiftSchedules.Remove(existing);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
