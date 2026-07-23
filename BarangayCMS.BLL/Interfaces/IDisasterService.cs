using System.Collections.Generic;
using System.Threading.Tasks;
using BarangayCMS.DTO;

namespace BarangayCMS.BLL.Interfaces
{
    public interface IDisasterService
    {
        Task<DisasterDTO?> GetDisasterByIdAsync(int id);
        Task<IEnumerable<DisasterDTO>> GetAllDisastersAsync();
        Task<IEnumerable<DisasterDTO>> GetActiveDisastersAsync();
        Task<bool> LogDisasterAsync(DisasterDTO dto);
        Task<bool> UpdateDisasterImpactAsync(DisasterDTO dto);
        Task<bool> CloseEvacuationCenterAsync(int id, string loggedBy);
    }
}