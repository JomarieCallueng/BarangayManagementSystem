using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BarangayCMS.DAL.Context;
using BarangayCMS.Entities;
using BarangayCMS.DAL.Repository.Interfaces;

namespace BarangayCMS.DAL.Repository
{
    public class EnvironmentRepository : IEnvironmentRepository
    {
        private readonly ApplicationDbContext _context;

        public EnvironmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<EnvironmentRecord?> GetByIdAsync(int id) =>
            await _context.EnvironmentRecords.FindAsync(id);

        public async Task<IEnumerable<EnvironmentRecord>> GetAllAsync() =>
            await _context.EnvironmentRecords.OrderByDescending(e => e.InspectionOrActivityDate).ToListAsync();

        public async Task<IEnumerable<EnvironmentRecord>> GetByLocationAsync(string location) =>
            await _context.EnvironmentRecords
                .Where(e => e.LocationArea.Contains(location))
                .ToListAsync();

        public async Task AddAsync(EnvironmentRecord record) =>
            await _context.EnvironmentRecords.AddAsync(record);

        public void Update(EnvironmentRecord record) =>
            _context.EnvironmentRecords.Update(record);

        public void Delete(EnvironmentRecord record) =>
            _context.EnvironmentRecords.Remove(record);

        public async Task<bool> SaveChangesAsync() =>
            await _context.SaveChangesAsync() > 0;
    }
}