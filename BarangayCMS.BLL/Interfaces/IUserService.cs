using System.Collections.Generic;
using System.Threading.Tasks;
using BarangayCMS.DTO;

namespace BarangayCMS.BLL.Interfaces
{
    public interface IUserService
    {
        Task<UserDTO?> GetUserByIdAsync(string id);
        Task<UserDTO?> GetUserByUsernameAsync(string username);
        Task<IEnumerable<UserDTO>> GetAllUsersAsync();
        Task<bool> UpdateUserProfileAsync(UserDTO dto);
        Task<bool> ToggleUserStatusAsync(string id, bool isActive); // Para sa Block/Unblock account
        Task<bool> UpdateLastLoginAsync(string id);
    }
}