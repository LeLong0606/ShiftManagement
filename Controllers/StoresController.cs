using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftManagement.Data;
using ShiftManagement.DTOs;

namespace ShiftManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoresController : ControllerBase
    {
        private readonly ShiftManagementContext _context;

        public StoresController(ShiftManagementContext context)
        {
            _context = context;
        }

        // GET: api/Stores
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StoreDto>>> GetStores()
        {
            var stores = await _context.Stores
                .AsNoTracking()
                .Select(s => new StoreDto
                {
                    StoreID = s.StoreID,
                    StoreName = s.StoreName,
                    Address = s.Address,
                    Phone = s.Phone,
                    Departments = s.Departments
                        .Select(d => new DepartmentDto
                        {
                            DepartmentID = d.DepartmentID,
                            DepartmentName = d.DepartmentName
                        })
                        .ToList()
                })
                .ToListAsync();

            return Ok(stores);
        }

        // GET: api/Stores/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<StoreDto>> GetStore(int id)
        {
            var store = await _context.Stores
                .AsNoTracking()
                .Where(s => s.StoreID == id)
                .Select(s => new StoreDto
                {
                    StoreID = s.StoreID,
                    StoreName = s.StoreName,
                    Address = s.Address,
                    Phone = s.Phone,
                    Departments = s.Departments
                        .Select(d => new DepartmentDto
                        {
                            DepartmentID = d.DepartmentID,
                            DepartmentName = d.DepartmentName
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (store == null)
                return NotFound();

            return Ok(store);
        }

        // POST: api/Stores
        [HttpPost]
        public async Task<ActionResult<StoreDto>> PostStore([FromBody] StoreCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var store = new Models.Store
            {
                StoreName = dto.StoreName,
                Address = dto.Address,
                Phone = dto.Phone
            };

            _context.Stores.Add(store);
            await _context.SaveChangesAsync();

            // map lại thành DTO để trả về
            var resultDto = new StoreDto
            {
                StoreID = store.StoreID,
                StoreName = store.StoreName,
                Address = store.Address,
                Phone = store.Phone,
                Departments = new()
            };

            return CreatedAtAction(nameof(GetStore), new { id = store.StoreID }, resultDto);
        }

        // PUT: api/Stores/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutStore(int id, [FromBody] StoreUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var store = await _context.Stores.FindAsync(id);
            if (store == null)
                return NotFound();

            store.StoreName = dto.StoreName;
            store.Address = dto.Address;
            store.Phone = dto.Phone;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Stores/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteStore(int id)
        {
            var store = await _context.Stores.FindAsync(id);
            if (store == null)
                return NotFound();

            _context.Stores.Remove(store);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
