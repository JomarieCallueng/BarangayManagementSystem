using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BarangayCMS.DAL.Context;
using BarangayCMS.DAL.Repository.Interfaces;
using BarangayCMS.Entities;

namespace BarangayCMS.DAL.Repository
{
    public class ContactMessageRepository : IContactMessageRepository
    {
        private readonly ApplicationDbContext _context;

        public ContactMessageRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ContactMessage?> GetByIdAsync(int id)
        {
            return await _context.ContactMessages.FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<IEnumerable<ContactMessage>> GetAllAsync()
        {
            // Pinakabago sa itaas.
            return await _context.ContactMessages
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> CountUnreadAsync()
        {
            return await _context.ContactMessages.CountAsync(m => m.Status == "Unread");
        }

        public async Task AddAsync(ContactMessage message)
        {
            await _context.ContactMessages.AddAsync(message);
        }

        public void Update(ContactMessage message)
        {
            _context.ContactMessages.Update(message);
        }

        public void Remove(ContactMessage message)
        {
            _context.ContactMessages.Remove(message);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
