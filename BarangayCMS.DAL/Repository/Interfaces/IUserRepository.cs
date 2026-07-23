using System.Collections.Generic;
using System.Threading.Tasks;
using BarangayCMS.Entities;

namespace BarangayCMS.DAL.Repository.Interfaces
{
    public interface IUserRepository
    {
        Task<ApplicationUser?> GetByIdAsync(string id);
        Task<ApplicationUser?> GetByUsernameAsync(string username);
        Task<IEnumerable<ApplicationUser>> GetAllAsync();
        void Update(ApplicationUser user);
        void Delete(ApplicationUser user);
        Task<bool> SaveChangesAsync();
    }
}