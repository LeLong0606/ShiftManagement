using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftManagement.Data;
using ShiftManagement.DTOs;

namespace ShiftManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Director,TeamLeader")]
    public class DepartmentsController : ControllerBase
    {
        private readonly ShiftManagementContext _context;

        public DepartmentsController(ShiftManagementContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DepartmentDto>>> GetDepartments()
        {
            var departments = await _context.Departments
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

            return Ok(departments);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DepartmentDto>> GetDepartment(int id)
        {
            var dept = await _context.Departments
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

            if (dept == null) return NotFound();
            return Ok(dept);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Director")]
        public async Task<ActionResult<DepartmentDto>> PostDepartment(DepartmentDto dto)
        {
            var dept = new Models.Department
            {
                DepartmentName = dto.DepartmentName,
                StoreID = dto.StoreID,
                ManagerID = dto.ManagerID
            };
            _context.Departments.Add(dept);
            await _context.SaveChangesAsync();

            dto.DepartmentID = dept.DepartmentID;
            // Optionally fetch StoreName from DB if you want to always return full info

            return CreatedAtAction(nameof(GetDepartment), new { id = dept.DepartmentID }, dto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Director")]
        public async Task<IActionResult> PutDepartment(int id, DepartmentDto dto)
        {
            if (id != dto.DepartmentID) return BadRequest();

            var dept = await _context.Departments.FindAsync(id);
            if (dept == null) return NotFound();

            dept.DepartmentName = dto.DepartmentName;
            dept.StoreID = dto.StoreID;
            dept.ManagerID = dto.ManagerID;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            var dept = await _context.Departments.FindAsync(id);
            if (dept == null) return NotFound();

            _context.Departments.Remove(dept);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}