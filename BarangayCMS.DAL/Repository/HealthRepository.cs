using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BarangayCMS.DAL.Context;
using BarangayCMS.Entities;
using BarangayCMS.DAL.Repository.Interfaces;

namespace BarangayCMS.DAL.Repository
{
    public class HealthRepository : IHealthRepository
    {
        private readonly ApplicationDbContext _context;

        public HealthRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<HealthRecord?> GetByIdAsync(int id) =>
            await _context.HealthRecords
                .Include(h => h.Resident)
                .FirstOrDefaultAsync(h => h.HealthRecordId == id);

        public async Task<IEnumerable<HealthRecord>> GetAllWithResidentAsync() =>
            await _context.HealthRecords
                .Include(h => h.Resident)
                .OrderByDescending(h => h.LastCheckupDate)
                .ToListAsync();

        public async Task<IEnumerable<HealthRecord>> GetByResidentIdAsync(int residentId) =>
            await _context.HealthRecords
                .Where(h => h.ResidentId == residentId)
                .OrderByDescending(h => h.LastCheckupDate)
                .ToListAsync();

        public async Task AddAsync(HealthRecord record) =>
            await _context.HealthRecords.AddAsync(record);

        public void Update(HealthRecord record) =>
            _context.HealthRecords.Update(record);

        public void Delete(HealthRecord record) =>
            _context.HealthRecords.Remove(record);

        public async Task<bool> SaveChangesAsync() =>
            await _context.SaveChangesAsync() > 0;
    }
}