using BarangayCMS.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BarangayCMS.DAL.Context
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Resident> Residents { get; set; } = null!;
        public DbSet<ReportLog> ReportLogs { get; set; } = null!;
        public DbSet<CertificateType> CertificateTypes { get; set; } = null!;

        // 📄 Dynamic na requirements per certificate type (admin-driven).
        public DbSet<CertificateRequirement> CertificateRequirements { get; set; } = null!;

        public DbSet<SystemSetting> SystemSettings { get; set; } = null!;

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
        public DbSet<BarangayOfficial> BarangayOfficials { get; set; } = null!;

        // 🏛️ Service history (mga termino) per barangay official.
        public DbSet<OfficialServiceHistory> OfficialServiceHistories { get; set; } = null!;

        // 🏛️ Standing Committees & Committee Assignments (SB & SK)
        public DbSet<Committee> Committees { get; set; } = null!;
        public DbSet<CommitteeAssignment> CommitteeAssignments { get; set; } = null!;

        // 📱 Emergency SMS Alert History (Disaster Risk Management module)
        public DbSet<SmsAlert> SmsAlerts { get; set; } = null!;

        // 💬 Contact Us messages (public → admin inbox). Walang account.
        public DbSet<ContactMessage> ContactMessages { get; set; } = null!;

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
            // Cascade delete: kapag natanggal ang uri ng sertipiko, kasama ang kanyang mga requirement.
            modelBuilder.Entity<CertificateRequirement>()
                .HasOne(r => r.CertificateType)
                .WithMany(t => t.Requirements)
                .HasForeignKey(r => r.CertificateTypeId)
                .OnDelete(DeleteBehavior.Cascade);

            // 🏛️ One Committee → many CommitteeAssignments.
            // Cascade delete: kapag natanggal ang komite, kasama ang mga assignments nito.
            modelBuilder.Entity<CommitteeAssignment>()
                .HasOne(ca => ca.Committee)
                .WithMany(c => c.Assignments)
                .HasForeignKey(ca => ca.CommitteeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}