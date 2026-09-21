using System.Globalization;
using BarangayCMS.BLL.Interfaces;
using BarangayCMS.BLL.Services;
using BarangayCMS.DAL.Context;
using BarangayCMS.DAL.Repository;
using BarangayCMS.DAL.Repository.Interfaces;
using BarangayCMS.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// 1. DATABASE CONFIGURATION
// ============================================================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ============================================================
// 2. DATA ACCESS LAYER (DAL) REPOSITORIES
// ============================================================
builder.Services.AddScoped<IResidentRepository, ResidentRepository>();
builder.Services.AddScoped<ICertificateRepository, CertificateRepository>();
builder.Services.AddScoped<IComplaintRepository, ComplaintRepository>();
builder.Services.AddScoped<IAnnouncementRepository, AnnouncementRepository>();
builder.Services.AddScoped<IBudgetRepository, BudgetRepository>();
builder.Services.AddScoped<IDisasterRepository, DisasterRepository>();
builder.Services.AddScoped<IEnvironmentRepository, EnvironmentRepository>();
builder.Services.AddScoped<IHealthRepository, HealthRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IContactMessageRepository, ContactMessageRepository>();
builder.Services.AddScoped<IEvacuationRepository, EvacuationRepository>();

// ============================================================
// 3. BUSINESS LOGIC LAYER (BLL) SERVICES
// ============================================================
builder.Services.AddScoped<IResidentService, ResidentService>();
builder.Services.AddScoped<ICertificateService, CertificateService>();
builder.Services.AddScoped<ICertificateRequirementService, CertificateRequirementService>();
builder.Services.AddScoped<IAnnouncementService, AnnouncementService>();
builder.Services.AddScoped<IBudgetService, BudgetService>();
builder.Services.AddScoped<IDisasterService, DisasterService>();
builder.Services.AddScoped<IComplaintService, ComplaintService>();
builder.Services.AddScoped<IEnvironmentService, EnvironmentService>();
builder.Services.AddScoped<IHealthService, HealthService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IContactMessageService, ContactMessageService>();
builder.Services.AddScoped<IEvacuationService, EvacuationService>();

// 📱 Semaphore SMS Integration Service
builder.Services.AddHttpClient<ISemaphoreService, SemaphoreService>();

// Helper for HttpContext access (Opsyonal ngunit inirerekomenda para sa Claims/User sessions)
builder.Services.AddHttpContextAccessor();

// ============================================================
// 4. IDENTITY & AUTHENTICATION CONFIGURATION
// ============================================================
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;

    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// ============================================================
// 5. LOCALIZATION (i18n) CONFIGURATION
// ============================================================
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.AddControllersWithViews(options =>
{
    // Umasa LAMANG sa tahasang [Required] attributes para sa non-nullable reference types.
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
})
.AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
.AddDataAnnotationsLocalization(options =>
{
    options.DataAnnotationLocalizerProvider = (type, factory) =>
        factory.Create(typeof(BarangayCMS.Web.SharedResource));
});

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("en"),   // English (default / fallback)
        new CultureInfo("fil")   // Filipino / Tagalog
    };
    options.DefaultRequestCulture = new RequestCulture("en");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;

    // Unahin ang Cookie provider para mag-persist ang napiling wika
    options.RequestCultureProviders.Insert(0, new CookieRequestCultureProvider());
});

var app = builder.Build();

// ============================================================
// 6. DATABASE MIGRATION & DEMO DATA SEEDING
// ============================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<ApplicationDbContext>();

        // Asynchronous migration para sa mas magandang performance
        await db.Database.MigrateAsync();

        // Patakbuhin ang DbSeeder
        await DbSeeder.SeedDemoDataAsync(db);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[SEED WARNING] Hindi natapos ang seeding: {ex.Message}");
    }
}

// ============================================================
// 7. MIDDLEWARE PIPELINE
// ============================================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// I-apply ang napiling culture sa bawat HTTP Request
app.UseRequestLocalization();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Area-based routes
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ============================================================
// 8. APPLICATION EXECUTION
// ============================================================
try
{
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine("========================================");
    Console.WriteLine($"CRASH ERROR: {ex.Message}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"DETAILS: {ex.InnerException.Message}");
    }
    Console.WriteLine("========================================");
    Console.ReadLine();
}