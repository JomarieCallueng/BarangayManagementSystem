using System.Collections.Generic;
using System.Threading.Tasks;
using BarangayCMS.DTO;

namespace BarangayCMS.BLL.Interfaces
{
    public interface IEnvironmentService
    {
        Task<EnvironmentDTO?> GetRecordByIdAsync(int id);
        Task<IEnumerable<EnvironmentDTO>> GetAllRecordsAsync();
        Task<bool> LogEnvironmentActivityAsync(EnvironmentDTO dto);
        Task<bool> UpdateEnvironmentRecordAsync(EnvironmentDTO dto);
        Task<bool> DeleteRecordAsync(int id);
    }
}