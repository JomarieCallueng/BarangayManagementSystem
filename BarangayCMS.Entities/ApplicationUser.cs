using System;
using Microsoft.AspNetCore.Identity;

namespace BarangayCMS.Entities
{
    // Nagmamana mula sa IdentityUser para automatic ang security features (Password, Email confirmation, etc.)
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // Admin, Staff, Resident
        public bool IsActive { get; set; } = true;
        public DateTime LastLoginTime { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.Now;
    }
}