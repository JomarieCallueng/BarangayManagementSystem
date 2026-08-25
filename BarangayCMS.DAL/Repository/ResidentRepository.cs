using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BarangayCMS.DAL.Context;
using BarangayCMS.Entities;
using BarangayCMS.DAL.Repository.Interfaces;

namespace BarangayCMS.DAL.Repository
{
    public class ResidentRepository : IResidentRepository
    {
        private readonly ApplicationDbContext _context;

        public ResidentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Resident?> GetByIdAsync(int id) =>
            await _context.Residents.FindAsync(id);

        public async Task<IEnumerable<Resident>> GetAllAsync() =>
            await _context.Residents.OrderBy(r => r.LastName).ThenBy(r => r.FirstName).ToListAsync();

        public async Task<IEnumerable<Resident>> GetVotersAsync() =>
            await _context.Residents.Where(r => r.IsVoter && r.IsResident).OrderBy(r => r.LastName).ToListAsync();

        public async Task AddAsync(Resident resident) =>
            await _context.Residents.AddAsync(resident);

        public void Update(Resident resident) =>
            _context.Residents.Update(resident);

        public void Delete(Resident resident) =>
            _context.Residents.Remove(resident);

        public async Task<bool> SaveChangesAsync() =>
            await _context.SaveChangesAsync() > 0;
    }
}