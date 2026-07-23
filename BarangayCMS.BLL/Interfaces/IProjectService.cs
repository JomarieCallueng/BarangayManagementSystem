using System.Collections.Generic;
using System.Threading.Tasks;
using BarangayCMS.DTO;

namespace BarangayCMS.BLL.Interfaces
{
    public interface IProjectService
    {
        Task<ProjectDTO?> GetProjectByIdAsync(int id);
        Task<IEnumerable<ProjectDTO>> GetAllProjectsAsync();
        Task<IEnumerable<ProjectDTO>> GetProjectsByStatusAsync(string status);
        Task<bool> ProposeProjectAsync(ProjectDTO dto);
        Task<bool> UpdateProjectDetailsAsync(ProjectDTO dto);
        Task<bool> UpdateExpensesAsync(int id, decimal additionalExpense);
        Task<bool> DeleteProjectAsync(int id);
    }
}