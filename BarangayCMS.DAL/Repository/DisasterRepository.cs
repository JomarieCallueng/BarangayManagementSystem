using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BarangayCMS.DAL.Context;
using BarangayCMS.Entities;
using BarangayCMS.DAL.Repository.Interfaces;

namespace BarangayCMS.DAL.Repository
{
    public class DisasterRepository : IDisasterRepository
    {
        private readonly ApplicationDbContext _context;

        public DisasterRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Disaster?> GetByIdAsync(int id) =>
            await _context.Disasters.FindAsync(id);

        public async Task<IEnumerable<Disaster>> GetAllAsync() =>
            await _context.Disasters.OrderByDescending(d => d.OccurrenceDate).ToListAsync();

        public async Task<IEnumerable<Disaster>> GetActiveIncidentsAsync() =>
            await _context.Disasters
                .Where(d => d.EvacuationCenterStatus == "Open" || d.ReliefDistributionStatus == "Ongoing")
                .OrderByDescending(d => d.OccurrenceDate)
                .ToListAsync();

        public async Task AddAsync(Disaster disaster) =>
            await _context.Disasters.AddAsync(disaster);

        public void Update(Disaster disaster) =>
            _context.Disasters.Update(disaster);

        public void Delete(Disaster disaster) =>
            _context.Disasters.Remove(disaster);

        public async Task<bool> SaveChangesAsync() =>
            await _context.SaveChangesAsync() > 0;
    }
}