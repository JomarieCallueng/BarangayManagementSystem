using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BarangayCMS.DAL.Context;
using BarangayCMS.Entities;
using BarangayCMS.DAL.Repository.Interfaces;

namespace BarangayCMS.DAL.Repository
{
    public class AnnouncementRepository : IAnnouncementRepository
    {
        private readonly ApplicationDbContext _context;

        public AnnouncementRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Announcement?> GetByIdAsync(int id) =>
            await _context.Announcements.FindAsync(id);

        public async Task<IEnumerable<Announcement>> GetAllAsync() =>
            await _context.Announcements.OrderByDescending(a => a.PublishDate).ToListAsync();

        public async Task<IEnumerable<Announcement>> GetPinnedAnnouncementsAsync() =>
            await _context.Announcements
                .Where(a => a.IsPinned)
                .OrderByDescending(a => a.PublishDate)
                .ToListAsync();

        public async Task AddAsync(Announcement announcement) =>
            await _context.Announcements.AddAsync(announcement);

        public void Update(Announcement announcement) =>
            _context.Announcements.Update(announcement);

        public void Delete(Announcement announcement) =>
            _context.Announcements.Remove(announcement);

        public async Task<bool> SaveChangesAsync() =>
            await _context.SaveChangesAsync() > 0;
    }
}