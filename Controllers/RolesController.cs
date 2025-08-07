using Microsoft.AspNetCore.Mvc;
using ShiftManagement.DTOs;
using ShiftManagement.Services;
using Microsoft.AspNetCore.Authorization;

namespace ShiftManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly RoleService _roleService;

        public RolesController(RoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<RoleDto>>> GetRoles(
            [FromQuery] string? search = "",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            var roles = await _roleService.GetRolesAsync(search, page, pageSize);
            return Ok(roles);
        }

        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<ActionResult<RoleDto>> GetRole(int id)
        {
            var dto = await _roleService.GetRoleAsync(id);
            if (dto == null)
                return NotFound();
            return Ok(dto);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<RoleDto>> PostRole([FromBody] RoleDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (resultDto, error) = await _roleService.CreateRoleAsync(dto);
            if (error != null)
                return Conflict(new { Message = error });

            return CreatedAtAction(nameof(GetRole), new { id = resultDto.RoleID }, resultDto);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutRole(int id, [FromBody] RoleDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var error = await _roleService.UpdateRoleAsync(id, dto);
            if (error == "Không tìm thấy role.")
                return NotFound();
            if (error != null)
                return Conflict(new { Message = error });

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var deleted = await _roleService.DeleteRoleAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}