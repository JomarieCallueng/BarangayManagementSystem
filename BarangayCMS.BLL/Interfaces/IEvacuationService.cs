using System.Collections.Generic;
using System.Threading.Tasks;
using BarangayCMS.DTO;

namespace BarangayCMS.BLL.Interfaces
{
    public interface IEvacuationService
    {
        // ---- Public (read-only) ----
        // Buong payload para sa Public Evacuation view.
        Task<PublicEvacuationDTO> GetPublicEvacuationInfoAsync();

        // ---- Admin/Staff — Centers ----
        Task<IEnumerable<EvacuationCenterDTO>> GetAllCentersAsync();
        Task<EvacuationCenterDTO?> GetCenterByIdAsync(int id);
        Task<bool> AddCenterAsync(EvacuationCenterDTO dto);
        Task<bool> UpdateCenterAsync(EvacuationCenterDTO dto);
        Task<bool> DeleteCenterAsync(int id);
        Task<bool> ToggleCenterActiveAsync(int id);

        // ---- Admin/Staff — Status ----
        Task<EvacuationStatusDTO> GetStatusAsync();
        Task<bool> UpdateStatusAsync(EvacuationStatusDTO dto);
    }
}
