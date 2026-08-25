using System.Collections.Generic;
using System.Threading.Tasks;
using BarangayCMS.DTO;

namespace BarangayCMS.BLL.Interfaces
{
    public interface IResidentService
    {
        Task<ResidentDTO?> GetResidentByIdAsync(int id);
        Task<IEnumerable<ResidentDTO>> GetAllResidentsAsync();
        Task<IEnumerable<ResidentDTO>> GetRegisteredVotersAsync();
        Task<bool> RegisterResidentAsync(ResidentDTO dto);
        Task<bool> UpdateResidentInfoAsync(ResidentDTO dto);
        Task<bool> ChangeResidentStatusAsync(int id, bool isActive); // Para sa Moved Out / Deceased status
    }
}