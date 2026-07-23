using System.Collections.Generic;
using System.Threading.Tasks;
using BarangayCMS.Entities;

namespace BarangayCMS.DAL.Repository.Interfaces
{
    public interface IHealthRepository
    {
        Task<HealthRecord?> GetByIdAsync(int id);
        Task<IEnumerable<HealthRecord>> GetAllWithResidentAsync();
        Task<IEnumerable<HealthRecord>> GetByResidentIdAsync(int residentId);
        Task AddAsync(HealthRecord record);
        void Update(HealthRecord record);
        void Delete(HealthRecord record);
        Task<bool> SaveChangesAsync();
    }
}