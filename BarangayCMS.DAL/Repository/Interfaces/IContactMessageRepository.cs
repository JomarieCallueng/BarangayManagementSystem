using System.Collections.Generic;
using System.Threading.Tasks;
using BarangayCMS.Entities;

namespace BarangayCMS.DAL.Repository.Interfaces
{
    public interface IContactMessageRepository
    {
        Task<ContactMessage?> GetByIdAsync(int id);
        Task<IEnumerable<ContactMessage>> GetAllAsync();
        Task<int> CountUnreadAsync();
        Task AddAsync(ContactMessage message);
        void Update(ContactMessage message);
        void Remove(ContactMessage message);
        Task<bool> SaveChangesAsync();
    }
}
