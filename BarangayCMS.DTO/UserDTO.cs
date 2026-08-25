using System;

namespace BarangayCMS.DTO
{
    public class UserDTO
    {
        public string Id { get; set; } = string.Empty; // Identity User GUID string
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty; // Admin, Staff, Resident
        public bool IsActive { get; set; }
        public DateTime LastLoginTime { get; set; }
    }
}
