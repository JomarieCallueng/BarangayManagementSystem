using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BarangayCMS.BLL.Interfaces;
using BarangayCMS.DAL.Repository.Interfaces;
using BarangayCMS.DTO;
using BarangayCMS.Entities;

namespace BarangayCMS.BLL.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;

        public UserService(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        public async Task<UserDTO?> GetUserByIdAsync(string id)
        {
            var u = await _userRepo.GetByIdAsync(id);
            if (u == null) return null;

            return new UserDTO
            {
                Id = u.Id,
                Username = u.UserName ?? string.Empty,
                Email = u.Email ?? string.Empty,
                FullName = u.FullName,
                Role = u.Role,
                IsActive = u.IsActive,
                LastLoginTime = u.LastLoginTime
            };
        }

        public async Task<UserDTO?> GetUserByUsernameAsync(string username)
        {
            var u = await _userRepo.GetByUsernameAsync(username);
            if (u == null) return null;

            return new UserDTO
            {
                Id = u.Id,
                Username = u.UserName ?? string.Empty,
                Email = u.Email ?? string.Empty,
                FullName = u.FullName,
                Role = u.Role,
                IsActive = u.IsActive,
                LastLoginTime = u.LastLoginTime
            };
        }

        public async Task<IEnumerable<UserDTO>> GetAllUsersAsync()
        {
            var users = await _userRepo.GetAllAsync();
            return users.Select(u => new UserDTO
            {
                Id = u.Id,
                Username = u.UserName ?? string.Empty,
                Email = u.Email ?? string.Empty,
                FullName = u.FullName,
                Role = u.Role,
                IsActive = u.IsActive,
                LastLoginTime = u.LastLoginTime
            });
        }

        public async Task<bool> UpdateUserProfileAsync(UserDTO dto)
        {
            var user = await _userRepo.GetByIdAsync(dto.Id);
            if (user == null) return false;

            // I-update ang profile details
            user.FullName = dto.FullName;
            user.Email = dto.Email;
            user.NormalizedEmail = dto.Email.ToUpper();
            user.Role = dto.Role;
            user.IsActive = dto.IsActive;

            _userRepo.Update(user);
            return await _userRepo.SaveChangesAsync();
        }

        public async Task<bool> ToggleUserStatusAsync(string id, bool isActive)
        {
            var user = await _userRepo.GetByIdAsync(id);
            if (user == null) return false;

            user.IsActive = isActive;

            _userRepo.Update(user);
            return await _userRepo.SaveChangesAsync();
        }

        public async Task<bool> UpdateLastLoginAsync(string id)
        {
            var user = await _userRepo.GetByIdAsync(id);
            if (user == null) return false;

            user.LastLoginTime = DateTime.Now;

            _userRepo.Update(user);
            return await _userRepo.SaveChangesAsync();
        }
    }
}