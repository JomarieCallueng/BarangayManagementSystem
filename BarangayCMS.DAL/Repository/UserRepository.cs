using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BarangayCMS.DAL.Context;
using BarangayCMS.Entities;
using BarangayCMS.DAL.Repository.Interfaces;

namespace BarangayCMS.DAL.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApplicationUser?> GetByIdAsync(string id) =>
            await _context.Users.FindAsync(id);

        public async Task<ApplicationUser?> GetByUsernameAsync(string username) =>
            await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);

        public async Task<IEnumerable<ApplicationUser>> GetAllAsync() =>
            await _context.Users.OrderBy(u => u.FullName).ToListAsync();

        public void Update(ApplicationUser user) =>
            _context.Users.Update(user);

        public void Delete(ApplicationUser user) =>
            _context.Users.Remove(user);

        public async Task<bool> SaveChangesAsync() =>
            await _context.SaveChangesAsync() > 0;
    }
}