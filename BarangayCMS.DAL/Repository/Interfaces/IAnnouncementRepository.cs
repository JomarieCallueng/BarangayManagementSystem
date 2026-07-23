using System.Collections.Generic;
using System.Threading.Tasks;
using BarangayCMS.Entities;

namespace BarangayCMS.DAL.Repository.Interfaces
{
    public interface IAnnouncementRepository
    {
        Task<Announcement?> GetByIdAsync(int id);
        Task<IEnumerable<Announcement>> GetAllAsync();
        Task<IEnumerable<Announcement>> GetPinnedAnnouncementsAsync();
        Task AddAsync(Announcement announcement);
        void Update(Announcement announcement);
        void Delete(Announcement announcement);
        Task<bool> SaveChangesAsync();
    }
}