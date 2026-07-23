using System.Collections.Generic;
using System.Threading.Tasks;
using BarangayCMS.DTO;

namespace BarangayCMS.BLL.Interfaces
{
    public interface IAnnouncementService
    {
        Task<AnnouncementDTO?> GetAnnouncementByIdAsync(int id);
        Task<IEnumerable<AnnouncementDTO>> GetAllAnnouncementsAsync();
        Task<IEnumerable<AnnouncementDTO>> GetHeroAnnouncementsAsync(); // Para sa mga Pinned/Banners
        Task<bool> CreateAnnouncementAsync(AnnouncementDTO dto);
        Task<bool> UpdateAnnouncementAsync(AnnouncementDTO dto);
        Task<bool> DeleteAnnouncementAsync(int id);
    }
}