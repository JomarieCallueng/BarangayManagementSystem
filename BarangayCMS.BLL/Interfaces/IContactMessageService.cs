using System.Collections.Generic;
using System.Threading.Tasks;
using BarangayCMS.DTO;

namespace BarangayCMS.BLL.Interfaces
{
    public interface IContactMessageService
    {
        // Public: nagpadala ng mensahe mula sa Contact Us form.
        Task<bool> SubmitAsync(string contactNumber, string message);

        // Admin inbox
        Task<IEnumerable<ContactMessageDTO>> GetAllAsync();
        Task<ContactMessageDTO?> GetByIdAsync(int id);
        Task<int> GetUnreadCountAsync();

        Task<bool> MarkAsReadAsync(int id);
        Task<bool> ReplyAsync(int id, string reply);
        Task<bool> ArchiveAsync(int id);
        Task<bool> DeleteAsync(int id);
    }
}
