using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BarangayCMS.Web.Services;

namespace BarangayCMS.Web.Areas.Admin.Controllers
{
    // Help System + User Manual para sa Admin. Static/informational lamang — walang
    // duplicate na help system, reused ang existing layout at design.
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class HelpController : Controller
    {
        // GET: /Admin/Help — Help System (ano ang ginagawa ng bawat module)
        public IActionResult Index() => View();

        // GET: /Admin/Help/Manual — Dina-download ang User Manual bilang tunay na PDF
        // file (attachment). Ang 3-argument na File(...) ay nagtatakda ng
        // "Content-Disposition: attachment; filename=User-Manual.pdf" kaya ito ay
        // dina-download, hindi HTML. Iisang PDF lamang ang ginagamit ng Admin at Staff.
        public IActionResult Manual()
            => File(UserManualPdf.GetBytes(), UserManualPdf.ContentType, UserManualPdf.FileName);
    }
}
