using Microsoft.AspNetCore.Mvc;
using ShiftManagement.DTOs;
using ShiftManagement.Services;

namespace ShiftManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HolidaysController : ControllerBase
    {
        private readonly HolidayService _holidayService;

        public HolidaysController(HolidayService holidayService)
        {
            _holidayService = holidayService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<HolidayDto>>> GetHolidays()
        {
            var list = await _holidayService.GetHolidaysAsync();
            return Ok(list);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<HolidayDto>> GetHoliday(int id)
        {
            var dto = await _holidayService.GetHolidayAsync(id);
            if (dto == null)
                return NotFound();
            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult<HolidayDto>> PostHoliday([FromBody] HolidayCreateDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _holidayService.CreateHolidayAsync(createDto);
            return CreatedAtAction(nameof(GetHoliday), new { id = result.HolidayID }, result);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutHoliday(int id, [FromBody] HolidayUpdateDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _holidayService.UpdateHolidayAsync(id, updateDto);
            if (!updated)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteHoliday(int id)
        {
            var deleted = await _holidayService.DeleteHolidayAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}