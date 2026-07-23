using BarangayCMS.DTO;

namespace BarangayCMS.BLL.Interfaces
{
    public interface IComplaintService
    {
        Task<ComplaintDTO?> GetComplaintByIdAsync(int id);
        Task<IEnumerable<ComplaintDTO>> GetAllComplaintsAsync();
        Task<IEnumerable<ComplaintDTO>> GetActiveComplaintsAsync();
        Task<bool> FileComplaintAsync(ComplaintDTO dto);
        Task<bool> UpdateComplaintStatusAsync(int id, string status, string remarks);
    }
}

