using BarangayCMS.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // NAPAKAHALAGA: Kailangan i-import ito
using Microsoft.EntityFrameworkCore;

namespace BarangayCMS.DAL.Context
{
    // 1. PALITAN ANG ': DbContext' NG ': IdentityDbContext<ApplicationUser>'
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Resident> Residents { get; set; } = null!;

        // 2. TINANGGAL/I-COMMENT OUT ITO: Hawak na ito ni IdentityDbContext sa background bilang 'Users'
        // public DbSet<ApplicationUser> Users { get; set; } = null!;

        public DbSet<ReportLog> ReportLogs { get; set; } = null!;
        public DbSet<CertificateType> CertificateTypes { get; set; }
        public DbSet<SystemSetting> SystemSettings { get; set; }

        public DbSet<Complaint> Complaints { get; set; } = null!;
        public DbSet<Certificate> Certificates { get; set; } = null!;
        public DbSet<Announcement> Announcements { get; set; } = null!;
        public DbSet<Budget> Budgets { get; set; } = null!;
        public DbSet<Disaster> Disasters { get; set; } = null!;
        public DbSet<EnvironmentRecord> EnvironmentRecords { get; set; } = null!;
        
        public DbSet<HealthRecord> HealthRecords { get; set; } = null!;
        
        public DbSet<Project> Projects { get; set; } = null!;
        public DbSet<BarangayOfficial> BarangayOfficials { get; set; }

        // 3. IDINAGDAG ITONG OVERRIDE METHOD (NAPAKAHALAGA)
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Tinatawag nito ang internal configuration ng Identity para i-setup ang mga tables tulad ng Claims, Roles, etc.
            base.OnModelCreating(modelBuilder);
        }
    }
}