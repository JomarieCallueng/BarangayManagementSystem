using System.Collections.Generic;
using System.Threading.Tasks;
using BarangayCMS.Entities;

namespace BarangayCMS.DAL.Repository.Interfaces
{
    public interface IBudgetRepository
    {
        Task<Budget?> GetByIdAsync(int id);
        Task<IEnumerable<Budget>> GetAllAsync();
        Task<IEnumerable<Budget>> GetByYearAsync(int year);
        Task AddAsync(Budget budget);
        void Update(Budget budget);
        void Delete(Budget budget);
        Task<bool> SaveChangesAsync();
    }
}