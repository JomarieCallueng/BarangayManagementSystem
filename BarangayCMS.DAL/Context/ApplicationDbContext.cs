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

        // 📄 Dynamic na requirements per certificate type (admin-driven).
        public DbSet<CertificateRequirement> CertificateRequirements { get; set; } = null!;

        public DbSet<SystemSetting> SystemSettings { get; set; }

        public DbSet<Complaint> Complaints { get; set; } = null!;
        public DbSet<Certificate> Certificates { get; set; } = null!;
        public DbSet<Announcement> Announcements { get; set; } = null!;
        public DbSet<Budget> Budgets { get; set; } = null!;
        public DbSet<Disaster> Disasters { get; set; } = null!;

        // 🏫 Evacuation module — mga pisikal na center at ang buong-barangay na status.
        public DbSet<EvacuationCenter> EvacuationCenters { get; set; } = null!;
        public DbSet<EvacuationStatus> EvacuationStatuses { get; set; } = null!;
        public DbSet<EnvironmentRecord> EnvironmentRecords { get; set; } = null!;
        
        public DbSet<HealthRecord> HealthRecords { get; set; } = null!;
        
        public DbSet<Project> Projects { get; set; } = null!;
        public DbSet<BarangayOfficial> BarangayOfficials { get; set; }

        // 🏛️ Service history (mga termino) per barangay official.
        public DbSet<OfficialServiceHistory> OfficialServiceHistories { get; set; } = null!;

        // 📱 Emergency SMS Alert History (Disaster Risk Management module)
        public DbSet<SmsAlert> SmsAlerts { get; set; } = null!;

        // 💬 Contact Us messages (public → admin inbox). Walang account.
        public DbSet<ContactMessage> ContactMessages { get; set; } = null!;

        // 3. IDINAGDAG ITONG OVERRIDE METHOD (NAPAKAHALAGA)
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Tinatawag nito ang internal configuration ng Identity para i-setup ang mga tables tulad ng Claims, Roles, etc.
            base.OnModelCreating(modelBuilder);

            // 🏛️ One person (BarangayOfficial) → many service-history records.
            // Cascade delete: kapag natanggal ang opisyal, kasama ang kanyang history.
            modelBuilder.Entity<OfficialServiceHistory>()
                .HasOne(h => h.BarangayOfficial)
                .WithMany(o => o.ServiceHistories)
                .HasForeignKey(h => h.BarangayOfficialId)
                .OnDelete(DeleteBehavior.Cascade);

            // 📄 One CertificateType → many CertificateRequirement.
            // Cascade delete: kapag natanggal ang uri ng sertipiko, kasama ang
            // kanyang mga requirement.
            modelBuilder.Entity<CertificateRequirement>()
                .HasOne(r => r.CertificateType)
                .WithMany(t => t.Requirements)
                .HasForeignKey(r => r.CertificateTypeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}