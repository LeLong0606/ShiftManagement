using Microsoft.EntityFrameworkCore;
using ShiftManagement.Data;
using ShiftManagement.DTOs;
using ShiftManagement.Models;

namespace ShiftManagement.Services
{
    public class HolidayService
    {
        private readonly ShiftManagementContext _context;

        public HolidayService(ShiftManagementContext context)
        {
            _context = context;
        }

        public async Task<List<HolidayDto>> GetHolidaysAsync()
        {
            return await _context.Holidays
                .AsNoTracking()
                .Include(h => h.DefaultShiftCode)
                .Select(h => new HolidayDto
                {
                    HolidayID = h.HolidayID,
                    Date = h.Date,
                    DefaultShiftCodeID = h.DefaultShiftCodeID ?? 0,
                    DefaultShiftCode = h.DefaultShiftCode != null ? h.DefaultShiftCode.Code : string.Empty
                })
                .ToListAsync();
        }

        public async Task<HolidayDto?> GetHolidayAsync(int id)
        {
            var h = await _context.Holidays
                .AsNoTracking()
                .Include(h => h.DefaultShiftCode)
                .FirstOrDefaultAsync(h => h.HolidayID == id);

            if (h == null) return null;

            return new HolidayDto
            {
                HolidayID = h.HolidayID,
                Date = h.Date,
                DefaultShiftCodeID = h.DefaultShiftCodeID ?? 0,
                DefaultShiftCode = h.DefaultShiftCode != null ? h.DefaultShiftCode.Code : string.Empty
            };
        }

        public async Task<HolidayDto> CreateHolidayAsync(HolidayCreateDto createDto)
        {
            var entity = new Holiday
            {
                Date = createDto.Date,
                DefaultShiftCodeID = createDto.DefaultShiftCodeID
            };

            _context.Holidays.Add(entity);
            await _context.SaveChangesAsync();

            await _context.Entry(entity)
                .Reference(h => h.DefaultShiftCode)
                .LoadAsync();

            return new HolidayDto
            {
                HolidayID = entity.HolidayID,
                Date = entity.Date,
                DefaultShiftCodeID = entity.DefaultShiftCodeID ?? 0,
                DefaultShiftCode = entity.DefaultShiftCode != null ? entity.DefaultShiftCode.Code : string.Empty
            };
        }

        public async Task<bool> UpdateHolidayAsync(int id, HolidayUpdateDto updateDto)
        {
            if (id != updateDto.HolidayID)
                return false;

            var existing = await _context.Holidays.FindAsync(id);
            if (existing == null)
                return false;

            existing.Date = updateDto.Date;
            existing.DefaultShiftCodeID = updateDto.DefaultShiftCodeID;

            _context.Entry(existing).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteHolidayAsync(int id)
        {
            var existing = await _context.Holidays.FindAsync(id);
            if (existing == null)
                return false;

            _context.Holidays.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}