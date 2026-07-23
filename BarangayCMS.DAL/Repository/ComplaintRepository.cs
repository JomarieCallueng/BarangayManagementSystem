using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BarangayCMS.DAL.Context;
using BarangayCMS.Entities;
using BarangayCMS.DAL.Repository.Interfaces;

namespace BarangayCMS.DAL.Repository
{
    public class ComplaintRepository : IComplaintRepository
    {
        private readonly ApplicationDbContext _context;

        public ComplaintRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Complaint?> GetByIdAsync(int id)
        {
            return await _context.Complaints
                .Include(c => c.Resident)
                .FirstOrDefaultAsync(c => c.ComplaintId == id);
        }

        public async Task<IEnumerable<Complaint>> GetAllAsync()
        {
            return await _context.Complaints
                .Include(c => c.Resident)
                .ToListAsync();
        }

        public async Task<IEnumerable<Complaint>> GetActiveComplaintsAsync()
        {
            return await _context.Complaints
                .Include(c => c.Resident)
                .Where(c => c.Status != "Resolved" && c.Status != "Dismissed")
                .ToListAsync();
        }

        public async Task AddAsync(Complaint complaint)
        {
            await _context.Complaints.AddAsync(complaint);
        }

        public void Update(Complaint complaint)
        {
            _context.Complaints.Update(complaint);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}