using BarangayCMS.BLL.Interfaces;
using BarangayCMS.BLL.Services;
using BarangayCMS.DAL.Context;
using BarangayCMS.DAL.Repository;
using BarangayCMS.DAL.Repository.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Data Access Layer (DAL) Repositories
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

// Business Logic Layer (BLL) Services
builder.Services.AddScoped<IResidentService, ResidentService>();
builder.Services.AddScoped<ICertificateService, CertificateService>();
builder.Services.AddScoped<IAnnouncementService, AnnouncementService>();
builder.Services.AddScoped<IBudgetService, BudgetService>();
builder.Services.AddScoped<IDisasterService, DisasterService>();
builder.Services.AddScoped<IComplaintService, ComplaintService>();
builder.Services.AddScoped<IEnvironmentService, EnvironmentService>();
builder.Services.AddScoped<IHealthService, HealthService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IUserService, UserService>();

// 📱 Semaphore SMS Integration Service Registration
builder.Services.AddHttpClient<ISemaphoreService, SemaphoreService>();

// Identity Configuration
builder.Services.AddIdentity<BarangayCMS.Entities.ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;

    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Auto-open Chrome (falls back to the default browser) once the app is listening.
// Only in Development so it never runs on a real server.
if (app.Environment.IsDevelopment())
{
    app.Lifetime.ApplicationStarted.Register(() =>
    {
        var url = app.Urls.FirstOrDefault() ?? "http://localhost:5199";
        try
        {
            // Try Google Chrome first.
            Process.Start(new ProcessStartInfo { FileName = "chrome", Arguments = url, UseShellExecute = true });
        }
        catch
        {
            try
            {
                // Chrome not found on PATH — open the system default browser instead.
                Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not auto-open a browser: {ex.Message}. Open {url} manually.");
            }
        }
    });
}

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