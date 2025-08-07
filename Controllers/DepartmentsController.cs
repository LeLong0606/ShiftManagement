using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftManagement.DTOs;
using ShiftManagement.Services;

namespace ShiftManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Director,TeamLeader")]
    public class DepartmentsController : ControllerBase
    {
        private readonly DepartmentService _departmentService;

        public DepartmentsController(DepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DepartmentDto>>> GetDepartments()
        {
            var departments = await _departmentService.GetDepartmentsAsync();
            return Ok(departments);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DepartmentDto>> GetDepartment(int id)
        {
            var dept = await _departmentService.GetDepartmentAsync(id);
            if (dept == null) return NotFound();
            return Ok(dept);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Director")]
        public async Task<ActionResult<DepartmentDto>> PostDepartment(DepartmentDto dto)
        {
            var createdDept = await _departmentService.CreateDepartmentAsync(dto);
            return CreatedAtAction(nameof(GetDepartment), new { id = createdDept.DepartmentID }, createdDept);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Director")]
        public async Task<IActionResult> PutDepartment(int id, DepartmentDto dto)
        {
            var updated = await _departmentService.UpdateDepartmentAsync(id, dto);
            if (!updated) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            var deleted = await _departmentService.DeleteDepartmentAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}