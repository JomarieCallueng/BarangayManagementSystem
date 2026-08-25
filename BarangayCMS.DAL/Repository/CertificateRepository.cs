using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BarangayCMS.DAL.Context;
using BarangayCMS.DAL.Repository.Interfaces;
using BarangayCMS.Entities;
using Microsoft.EntityFrameworkCore;

namespace BarangayCMS.DAL.Repository
{
    public class CertificateRepository : ICertificateRepository
    {
        private readonly ApplicationDbContext _context;

        public CertificateRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Certificate?> GetByIdAsync(int id) =>
            await _context.Certificates
                .Include(c => c.Resident)
                .FirstOrDefaultAsync(c => c.CertificateId == id);

        public async Task<IEnumerable<Certificate>> GetAllWithResidentAsync() =>
            await _context.Certificates
                .Include(c => c.Resident)
                .OrderByDescending(c => c.CertificateId) // 💡 I-sort para nasa ITAAS agad ang pinakabagong gawa
                .AsNoTracking()
                .ToListAsync();

        public async Task<IEnumerable<Certificate>> GetByResidentIdAsync(int residentId) =>
            await _context.Certificates
                .Where(c => c.ResidentId == residentId)
                .OrderByDescending(c => c.CertificateId)
                .ToListAsync();

        public async Task AddAsync(Certificate certificate) =>
            await _context.Certificates.AddAsync(certificate);

        public void Update(Certificate certificate) =>
            _context.Certificates.Update(certificate);

        public async Task<bool> SaveChangesAsync() =>
            await _context.SaveChangesAsync() > 0;
    }
}