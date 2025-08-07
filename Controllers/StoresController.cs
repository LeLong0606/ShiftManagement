using Microsoft.AspNetCore.Mvc;
using ShiftManagement.DTOs;
using ShiftManagement.Services;
using Microsoft.AspNetCore.Authorization;

namespace ShiftManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoresController : ControllerBase
    {
        private readonly StoreService _storeService;

        public StoresController(StoreService storeService)
        {
            _storeService = storeService;
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
            var stores = await _storeService.GetStoresAsync(search, page, pageSize);
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
            var dto = await _storeService.GetStoreAsync(id);
            if (dto == null)
                return NotFound();
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

            var resultDto = await _storeService.CreateStoreAsync(dto);

            return CreatedAtAction(nameof(GetStore), new { id = resultDto.StoreID }, resultDto);
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

            var updated = await _storeService.UpdateStoreAsync(id, dto);
            if (!updated)
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// [DELETE] Xóa cửa hàng theo ID. Chỉ Admin có quyền xóa.
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteStore(int id)
        {
            var deleted = await _storeService.DeleteStoreAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}