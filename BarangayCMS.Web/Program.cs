using BarangayCMS.BLL.Interfaces;
using BarangayCMS.BLL.Services;
using BarangayCMS.DAL.Context;
using BarangayCMS.DAL.Repository;
using BarangayCMS.DAL.Repository.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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
builder.Services.AddScoped<IContactMessageRepository, ContactMessageRepository>();

// Business Logic Layer (BLL) Services
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

builder.Services.AddControllersWithViews(options =>
{
    // Umasa LAMANG sa tahasang [Required] attributes. Kung hindi, ang mga
    // non-nullable string properties (hal. SignaturePath, ProfileImagePath,
    // Committee) ay tumatanggap ng implicit [Required] na tumatanggi sa
    // empty string — kaya tahimik na nabibigo ang pag-save kapag walang laman
    // ang field na iyon. (Root cause ng hindi ma-save na Edit forms.)
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});

var app = builder.Build();

// Punan ang lahat ng module ng demo data (idempotent — ligtas paulit-ulit).
// Ginagamit LAMANG ang tatlong pangalan: Jomarie Callueng,
// Mark Dave Casao Cardenas, at Klarence Alfred Z. Villar.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        db.Database.Migrate();
        await BarangayCMS.Web.Data.DbSeeder.SeedDemoDataAsync(db);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[SEED WARNING] Hindi natapos ang seeding: {ex.Message}");
    }
}

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

// NOTE: Ang browser ay awtomatikong binubuksan na ng "launchBrowser": true sa
// Properties/launchSettings.json. Huwag nang magdagdag ng sariling Process.Start
// dito dahil magbubukas iyon ng pangalawang tab/window.

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