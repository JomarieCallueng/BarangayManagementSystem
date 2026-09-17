using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BarangayCMS.DAL.Context;
using BarangayCMS.Entities;
using BarangayCMS.DAL.Repository.Interfaces;

namespace BarangayCMS.DAL.Repository
{
    public class EvacuationRepository : IEvacuationRepository
    {
        private readonly ApplicationDbContext _context;

        public EvacuationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // ---- Evacuation Centers ----

        public async Task<EvacuationCenter?> GetCenterByIdAsync(int id) =>
            await _context.EvacuationCenters.FindAsync(id);

        public async Task<IEnumerable<EvacuationCenter>> GetAllCentersAsync() =>
            await _context.EvacuationCenters
                .OrderByDescending(c => c.IsActive)
                .ThenBy(c => c.Name)
                .ToListAsync();

        public async Task<IEnumerable<EvacuationCenter>> GetActiveCentersAsync() =>
            await _context.EvacuationCenters
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();

        public async Task AddCenterAsync(EvacuationCenter center) =>
            await _context.EvacuationCenters.AddAsync(center);

        public void UpdateCenter(EvacuationCenter center) =>
            _context.EvacuationCenters.Update(center);

        public void DeleteCenter(EvacuationCenter center) =>
            _context.EvacuationCenters.Remove(center);

        // ---- Barangay-wide Evacuation Status (singleton) ----

        public async Task<EvacuationStatus?> GetStatusAsync() =>
            await _context.EvacuationStatuses.OrderBy(s => s.Id).FirstOrDefaultAsync();

        public async Task AddStatusAsync(EvacuationStatus status) =>
            await _context.EvacuationStatuses.AddAsync(status);

        public void UpdateStatus(EvacuationStatus status) =>
            _context.EvacuationStatuses.Update(status);

        public async Task<bool> SaveChangesAsync() =>
            await _context.SaveChangesAsync() > 0;
    }
}
