using Microsoft.EntityFrameworkCore;
using ShiftManagement.Data;
using ShiftManagement.DTOs;
using ShiftManagement.Models;

namespace ShiftManagement.Services
{
    public class StoreService
    {
        private readonly ShiftManagementContext _context;

        public StoreService(ShiftManagementContext context)
        {
            _context = context;
        }

        public async Task<List<StoreDto>> GetStoresAsync(string? search = "", int page = 1, int pageSize = 50)
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

            return stores;
        }

        public async Task<StoreDto?> GetStoreAsync(int id)
        {
            var store = await _context.Stores
                .AsNoTracking()
                .Include(s => s.Departments)
                .FirstOrDefaultAsync(s => s.StoreID == id);

            if (store == null)
                return null;

            return new StoreDto
            {
                StoreID = store.StoreID,
                StoreName = store.StoreName,
                Address = store.Address,
                Phone = store.Phone
            };
        }

        public async Task<StoreDto> CreateStoreAsync(StoreCreateDto dto)
        {
            var store = new Store
            {
                StoreName = dto.StoreName,
                Address = dto.Address,
                Phone = dto.Phone
            };

            _context.Stores.Add(store);
            await _context.SaveChangesAsync();

            return new StoreDto
            {
                StoreID = store.StoreID,
                StoreName = store.StoreName,
                Address = store.Address,
                Phone = store.Phone
            };
        }

        public async Task<bool> UpdateStoreAsync(int id, StoreUpdateDto dto)
        {
            var store = await _context.Stores.FindAsync(id);
            if (store == null)
                return false;

            store.StoreName = dto.StoreName;
            store.Address = dto.Address;
            store.Phone = dto.Phone;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteStoreAsync(int id)
        {
            var store = await _context.Stores.FindAsync(id);
            if (store == null)
                return false;

            _context.Stores.Remove(store);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}