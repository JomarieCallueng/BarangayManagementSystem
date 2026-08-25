using System.Collections.Generic;
using System.Threading.Tasks;
using BarangayCMS.Entities;

namespace BarangayCMS.DAL.Repository.Interfaces
{
    public interface IDisasterRepository
    {
        Task<Disaster?> GetByIdAsync(int id);
        Task<IEnumerable<Disaster>> GetAllAsync();
        Task<IEnumerable<Disaster>> GetActiveIncidentsAsync(); // Yung mga bukas pa ang Evacuation o ongoing ang relief
        Task AddAsync(Disaster disaster);
        void Update(Disaster disaster);
        void Delete(Disaster disaster);
        Task<bool> SaveChangesAsync();
    }
}