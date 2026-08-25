using System.Collections.Generic;
using System.Threading.Tasks;
using BarangayCMS.Entities;

namespace BarangayCMS.DAL.Repository.Interfaces
{
    public interface IResidentRepository
    {
        Task<Resident?> GetByIdAsync(int id);
        Task<IEnumerable<Resident>> GetAllAsync();
        Task<IEnumerable<Resident>> GetVotersAsync();
        Task AddAsync(Resident resident);
        void Update(Resident resident);
        void Delete(Resident resident);
        Task<bool> SaveChangesAsync();
    }
}