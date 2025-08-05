using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftManagement.Data;
using ShiftManagement.DTOs;
using ShiftManagement.Models;

namespace ShiftManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HolidaysController : ControllerBase
    {
        private readonly ShiftManagementContext _context;

        public HolidaysController(ShiftManagementContext context)
        {
            _context = context;
        }

        // GET: api/Holidays
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HolidayDto>>> GetHolidays()
        {
            var list = await _context.Holidays
                .AsNoTracking()
                .Include(h => h.DefaultShiftCode)
                .Select(h => new HolidayDto
                {
                    HolidayID = h.HolidayID,
                    Date = h.Date,
                    // ép nullable int về int với 0 là default nếu null
                    DefaultShiftCodeID = h.DefaultShiftCodeID ?? 0,
                    DefaultShiftCode = h.DefaultShiftCode != null ? h.DefaultShiftCode.Code : string.Empty
                })
                .ToListAsync();

            return Ok(list);
        }

        // GET: api/Holidays/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<HolidayDto>> GetHoliday(int id)
        {
            var h = await _context.Holidays
                .AsNoTracking()
                .Include(h => h.DefaultShiftCode)
                .FirstOrDefaultAsync(h => h.HolidayID == id);

            if (h == null)
                return NotFound();

            var dto = new HolidayDto
            {
                HolidayID = h.HolidayID,
                Date = h.Date,
                DefaultShiftCodeID = h.DefaultShiftCodeID ?? 0,
                DefaultShiftCode = h.DefaultShiftCode != null ? h.DefaultShiftCode.Code : string.Empty
            };
            return Ok(dto);
        }

        // POST: api/Holidays
        [HttpPost]
        public async Task<ActionResult<HolidayDto>> PostHoliday([FromBody] HolidayCreateDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var entity = new Holiday
            {
                Date = createDto.Date,
                DefaultShiftCodeID = createDto.DefaultShiftCodeID
            };

            _context.Holidays.Add(entity);
            await _context.SaveChangesAsync();

            // load navigation để lấy code
            await _context.Entry(entity)
                          .Reference(h => h.DefaultShiftCode)
                          .LoadAsync();

            var result = new HolidayDto
            {
                HolidayID = entity.HolidayID,
                Date = entity.Date,
                DefaultShiftCodeID = entity.DefaultShiftCodeID ?? 0,
                DefaultShiftCode = entity.DefaultShiftCode != null ? entity.DefaultShiftCode.Code : string.Empty
            };

            return CreatedAtAction(
                nameof(GetHoliday),
                new { id = result.HolidayID },
                result
            );
        }

        // PUT: api/Holidays/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutHoliday(int id, [FromBody] HolidayUpdateDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != updateDto.HolidayID)
                return BadRequest("ID không khớp.");

            var existing = await _context.Holidays.FindAsync(id);
            if (existing == null)
                return NotFound();

            existing.Date = updateDto.Date;
            existing.DefaultShiftCodeID = updateDto.DefaultShiftCodeID;

            _context.Entry(existing).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Holidays/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteHoliday(int id)
        {
            var existing = await _context.Holidays.FindAsync(id);
            if (existing == null)
                return NotFound();

            _context.Holidays.Remove(existing);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
