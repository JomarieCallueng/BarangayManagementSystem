using System.Collections.Generic;
using System.Threading.Tasks;
using BarangayCMS.Entities;

namespace BarangayCMS.DAL.Repository.Interfaces
{
    public interface IProjectRepository
    {
        Task<Project?> GetByIdAsync(int id);
        Task<IEnumerable<Project>> GetAllAsync();
        Task<IEnumerable<Project>> GetProjectsByStatusAsync(string status);
        Task AddAsync(Project project);
        void Update(Project project);
        void Delete(Project project);
        Task<bool> SaveChangesAsync();
    }
}