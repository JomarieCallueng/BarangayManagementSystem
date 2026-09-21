using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BarangayCMS.Entities;
using Microsoft.EntityFrameworkCore;

namespace BarangayCMS.DAL.Context
{
    public static class DbSeeder
    {
        public static async Task SeedDemoDataAsync(ApplicationDbContext context)
        {
            if (context == null) return;

            // 1. Seed Standing Committees (8 SB & 7 SK)
            await SeedCommitteesAsync(context);

            // 2. Seed Initial Barangay Officials (kung wala pa)
            await SeedOfficialsAsync(context);

            // 3. Seed Sample SMS Alerts History (gamit ang BarangayCMS.Entities.SmsAlert)
            await SeedSmsAlertsAsync(context);

            await context.SaveChangesAsync();
        }

        private static async Task SeedCommitteesAsync(ApplicationDbContext context)
        {
            if (await context.Committees.AnyAsync()) return;

            var committees = new List<Committee>
            {
                // --- Sangguniang Barangay (8 Standing Committees) ---
                new Committee { Name = "Appropriations & Finance", Code = "SB-FIN", Category = "SB", Description = "Pondo, badyet, at taunang gastusin ng barangay.", IconClass = "bi-cash-coin" },
                new Committee { Name = "Peace and Order & Public Safety", Code = "SB-PEACE", Category = "SB", Description = "Seguridad, Barangay Tanod, at kapayapaan sa komunidad.", IconClass = "bi-shield-check" },
                new Committee { Name = "Health and Sanitation", Code = "SB-HEALTH", Category = "SB", Description = "Health center, pasilidad ng kalusugan, at kalinisan ng kapaligiran.", IconClass = "bi-heart-pulse" },
                new Committee { Name = "Infrastructure & Public Works", Code = "SB-INFRA", Category = "SB", Description = "Kalsada, gusali, drainages, at mga pampublikong pasilidad.", IconClass = "bi-cone-striped" },
                new Committee { Name = "Education & Culture", Code = "SB-EDUC", Category = "SB", Description = "Mga paaralan, day care centers, at mga pangkulturang aktibidad.", IconClass = "bi-mortarboard" },
                new Committee { Name = "Environment & Agriculture", Code = "SB-ENV", Category = "SB", Description = "Waste management, pagtatanim, at pangangalaga sa kalikasan.", IconClass = "bi-tree" },
                new Committee { Name = "Women, Family & Social Services", Code = "SB-WOMEN", Category = "SB", Description = "Proteksyon sa kababaihan, bata, senior citizens, at PWDs.", IconClass = "bi-people" },
                new Committee { Name = "Youth & Sports Development", Code = "SB-YOUTH", Category = "SB", Description = "Karaniwang pinangungunahan ng SK Chairperson.", IconClass = "bi-dribbble" },

                // --- Sangguniang Kabataan (7 Standing Committees) ---
                new Committee { Name = "Education and Culture", Code = "SK-EDUC", Category = "SK", Description = "Scholarships, student assistance, and arts/cultural activities.", IconClass = "bi-mortarboard" },
                new Committee { Name = "Environmental Protection, Climate Change & DRRM", Code = "SK-ENV", Category = "SK", Description = "Environmental protection, tree planting, climate awareness, and disaster preparedness.", IconClass = "bi-tree" },
                new Committee { Name = "Youth Employment and Livelihood", Code = "SK-JOBS", Category = "SK", Description = "Livelihood training, job fairs, employment opportunities, and youth entrepreneurship.", IconClass = "bi-briefcase" },
                new Committee { Name = "Health, Health Services & Anti-Drug Abuse", Code = "SK-HEALTH", Category = "SK", Description = "Health awareness, adolescent health, mental health awareness, and anti-drug programs.", IconClass = "bi-heart-pulse" },
                new Committee { Name = "Gender and Development", Code = "SK-GAD", Category = "SK", Description = "Programs supporting gender equality, protection, inclusion, and youth welfare.", IconClass = "bi-people" },
                new Committee { Name = "Sports Development", Code = "SK-SPORTS", Category = "SK", Description = "Sports activities, leagues, recreation, and physical fitness programs.", IconClass = "bi-dribbble" },
                new Committee { Name = "Active Citizenship & Capability Building", Code = "SK-CIVIC", Category = "SK", Description = "Leadership training, volunteerism, civic participation, and youth governance.", IconClass = "bi-megaphone" }
            };

            await context.Committees.AddRangeAsync(committees);
        }

        private static async Task SeedOfficialsAsync(ApplicationDbContext context)
        {
            if (await context.BarangayOfficials.AnyAsync()) return;

            // Optional sample initial seeding for Barangay Officials
            var defaultOfficials = new List<BarangayOfficial>
            {
                new BarangayOfficial { FullName = "Punong Barangay Admin", Position = "Barangay Captain", IsActive = true },
                new BarangayOfficial { FullName = "SK Chair Leader", Position = "SK Chairperson", IsActive = true }
            };

            await context.BarangayOfficials.AddRangeAsync(defaultOfficials);
        }

        private static async Task SeedSmsAlertsAsync(ApplicationDbContext context)
        {
            if (await context.SmsAlerts.AnyAsync()) return;

            var sampleAlerts = new List<SmsAlert>
            {
                new SmsAlert
                {
                    EmergencyType = "Typhoon Warning",
                    Message = "MAG-INGAT: Malakas na ulan dahil sa Bagyo. Manatili sa loob ng bahay.",
                    RecipientGroup = "All Residents",
                    RecipientCount = 150,
                    SuccessCount = 148,
                    FailedCount = 2,
                    Status = "Sent",
                    SentBy = "Admin",
                    SentAt = DateTime.Now.AddDays(-1)
                }
            };

            await context.SmsAlerts.AddRangeAsync(sampleAlerts);
        }
    }
}