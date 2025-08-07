using Microsoft.EntityFrameworkCore;
using ShiftManagement.Data;
using ShiftManagement.DTOs;
using ShiftManagement.Models;

namespace ShiftManagement.Services
{
    public class DepartmentService
    {
        private readonly ShiftManagementContext _context;

        public DepartmentService(ShiftManagementContext context)
        {
            _context = context;
        }

        public async Task<List<DepartmentDto>> GetDepartmentsAsync()
        {
            return await _context.Departments
                .AsNoTracking()
                .Include(d => d.Store)
                .Select(d => new DepartmentDto
                {
                    DepartmentID = d.DepartmentID,
                    DepartmentName = d.DepartmentName,
                    StoreID = d.StoreID,
                    StoreName = d.Store.StoreName,
                    ManagerID = d.ManagerID
                })
                .ToListAsync();
        }

        public async Task<DepartmentDto?> GetDepartmentAsync(int id)
        {
            return await _context.Departments
                .AsNoTracking()
                .Include(d => d.Store)
                .Where(d => d.DepartmentID == id)
                .Select(d => new DepartmentDto
                {
                    DepartmentID = d.DepartmentID,
                    DepartmentName = d.DepartmentName,
                    StoreID = d.StoreID,
                    StoreName = d.Store.StoreName,
                    ManagerID = d.ManagerID
                })
                .FirstOrDefaultAsync();
        }

        public async Task<DepartmentDto> CreateDepartmentAsync(DepartmentDto dto)
        {
            var dept = new Department
            {
                DepartmentName = dto.DepartmentName,
                StoreID = dto.StoreID,
                ManagerID = dto.ManagerID
            };
            _context.Departments.Add(dept);
            await _context.SaveChangesAsync();

            dto.DepartmentID = dept.DepartmentID;
            // Optionally fill StoreName if required (fetch from db if not provided)
            return dto;
        }

        public async Task<bool> UpdateDepartmentAsync(int id, DepartmentDto dto)
        {
            if (id != dto.DepartmentID) return false;

            var dept = await _context.Departments.FindAsync(id);
            if (dept == null) return false;

            dept.DepartmentName = dto.DepartmentName;
            dept.StoreID = dto.StoreID;
            dept.ManagerID = dto.ManagerID;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteDepartmentAsync(int id)
        {
            var dept = await _context.Departments.FindAsync(id);
            if (dept == null) return false;

            _context.Departments.Remove(dept);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}