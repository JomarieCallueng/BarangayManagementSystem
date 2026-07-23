using System.Collections.Generic;
using System.Threading.Tasks;
using BarangayCMS.Entities;

namespace BarangayCMS.DAL.Repository.Interfaces
{
    public interface IComplaintRepository
    {
        Task<Complaint?> GetByIdAsync(int id);
        Task<IEnumerable<Complaint>> GetAllAsync();

        // Dagdag na method para sa Active complaints filter kung gusto mo sa repo level, 
        // o pwede mo rin i-filter sa service level gaya ng code sa ibaba.
        Task<IEnumerable<Complaint>> GetActiveComplaintsAsync();

        Task AddAsync(Complaint complaint);
        void Update(Complaint complaint);
        Task<bool> SaveChangesAsync();
    }
}