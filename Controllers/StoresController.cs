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
    public class StoresController : ControllerBase
    {
        private readonly ShiftManagementContext _context;

        public StoresController(ShiftManagementContext context)
        {
            _context = context;
        }

        /// <summary>
        /// [GET] Lấy danh sách cửa hàng. Có thể phân trang và tìm kiếm.
        /// Chỉ tài khoản đã đăng nhập mới truy cập được.
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<StoreDto>>> GetStores(
            [FromQuery] string? search = "",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 50;

            var query = _context.Stores.AsNoTracking();

            if (!string.IsNullOrEmpty(search))
            {
                string pattern = $"%{search}%";
                query = query.Where(s =>
                    EF.Functions.Like(s.StoreName, pattern) ||
                    (s.Address != null && EF.Functions.Like(s.Address, pattern)) ||
                    (s.Phone != null && EF.Functions.Like(s.Phone, pattern)));
            }

            var stores = await query
                .OrderBy(s => s.StoreID)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new StoreDto
                {
                    StoreID = s.StoreID,
                    StoreName = s.StoreName,
                    Address = s.Address,
                    Phone = s.Phone
                })
                .ToListAsync();

            return Ok(stores);
        }

        /// <summary>
        /// [GET] Lấy thông tin chi tiết một cửa hàng theo ID.
        /// Chỉ tài khoản đã đăng nhập mới truy cập được.
        /// </summary>
        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<ActionResult<StoreDto>> GetStore(int id)
        {
            var store = await _context.Stores
                .AsNoTracking()
                .Include(s => s.Departments)
                .FirstOrDefaultAsync(s => s.StoreID == id);

            if (store == null)
                return NotFound();

            var dto = new StoreDto
            {
                StoreID = store.StoreID,
                StoreName = store.StoreName,
                Address = store.Address,
                Phone = store.Phone
            };

            return Ok(dto);
        }

        /// <summary>
        /// [POST] Tạo mới một cửa hàng. Chỉ Admin có quyền tạo.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<StoreDto>> PostStore([FromBody] StoreCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var store = new Store
            {
                StoreName = dto.StoreName,
                Address = dto.Address,
                Phone = dto.Phone
            };

            _context.Stores.Add(store);
            await _context.SaveChangesAsync();

            var resultDto = new StoreDto
            {
                StoreID = store.StoreID,
                StoreName = store.StoreName,
                Address = store.Address,
                Phone = store.Phone
            };

            return CreatedAtAction(nameof(GetStore), new { id = store.StoreID }, resultDto);
        }

        /// <summary>
        /// [PUT] Cập nhật thông tin cửa hàng theo ID. Chỉ Admin có quyền sửa.
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
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

        /// <summary>
        /// [DELETE] Xóa cửa hàng theo ID. Chỉ Admin có quyền xóa.
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
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