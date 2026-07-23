using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BarangayCMS.DAL.Context;
using BarangayCMS.Entities;
using BarangayCMS.DAL.Repository.Interfaces;

namespace BarangayCMS.DAL.Repository
{
    public class ReportRepository : IReportRepository
    {
        private readonly ApplicationDbContext _context;

        public ReportRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ReportLog?> GetByIdAsync(int id) =>
            await _context.ReportLogs.FindAsync(id);

        public async Task<IEnumerable<ReportLog>> GetAllLogsAsync() =>
            await _context.ReportLogs.OrderByDescending(r => r.GeneratedAt).ToListAsync();

        public async Task AddLogAsync(ReportLog log) =>
            await _context.ReportLogs.AddAsync(log);

        public async Task<bool> SaveChangesAsync() =>
            await _context.SaveChangesAsync() > 0;
    }
}