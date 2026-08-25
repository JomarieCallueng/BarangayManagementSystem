using System.Collections.Generic;
using System.Threading.Tasks;
using BarangayCMS.Entities;

namespace BarangayCMS.DAL.Repository.Interfaces
{
    public interface ICertificateRepository
    {
        Task<Certificate?> GetByIdAsync(int id);
        Task<IEnumerable<Certificate>> GetAllWithResidentAsync();
        Task<IEnumerable<Certificate>> GetByResidentIdAsync(int residentId);
        Task AddAsync(Certificate certificate);
        void Update(Certificate certificate);
        Task<bool> SaveChangesAsync();
    }
}