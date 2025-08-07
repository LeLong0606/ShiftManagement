using Microsoft.EntityFrameworkCore;
using ShiftManagement.Data;
using ShiftManagement.DTOs;
using ShiftManagement.Models;

namespace ShiftManagement.Services
{
    public class ScheduleService
    {
        private readonly ShiftManagementContext _context;

        public ScheduleService(ShiftManagementContext context)
        {
            _context = context;
        }

        public async Task<List<ShiftScheduleDto>> GetSchedulesAsync(
            DateTime? from,
            DateTime? to,
            int? departmentId,
            int page = 1,
            int pageSize = 50)
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

            return result;
        }

        public async Task<ShiftScheduleDto?> CreateScheduleAsync(ShiftScheduleCreateDto dto, int createdById)
        {
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

            return result;
        }

        public async Task<bool> UpdateScheduleAsync(int id, ShiftScheduleUpdateDto dto)
        {
            var entity = await _context.ShiftSchedules
                .Include(s => s.ShiftScheduleDetails)
                .FirstOrDefaultAsync(s => s.ScheduleID == id);

            if (entity == null)
                return false;

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
            return true;
        }

        public async Task<bool> DeleteScheduleAsync(int id)
        {
            var entity = await _context.ShiftSchedules.FindAsync(id);
            if (entity == null)
                return false;

            _context.ShiftSchedules.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}