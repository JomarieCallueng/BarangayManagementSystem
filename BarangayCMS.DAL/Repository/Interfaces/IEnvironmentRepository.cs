using System.Collections.Generic;
using System.Threading.Tasks;
using BarangayCMS.Entities;

namespace BarangayCMS.DAL.Repository.Interfaces
{
    public interface IEnvironmentRepository
    {
        Task<EnvironmentRecord?> GetByIdAsync(int id);
        Task<IEnumerable<EnvironmentRecord>> GetAllAsync();
        Task<IEnumerable<EnvironmentRecord>> GetByLocationAsync(string location);
        Task AddAsync(EnvironmentRecord record);
        void Update(EnvironmentRecord record);
        void Delete(EnvironmentRecord record);
        Task<bool> SaveChangesAsync();
    }
}
