using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BarangayCMS.Web.Services;

namespace BarangayCMS.Web.Areas.Staff.Controllers
{
    // Help System + User Manual para sa Staff/Encoder. Static/informational lamang.
    [Area("Staff")]
    [Authorize(Roles = "Staff,Staff / Encoder")]
    public class HelpController : Controller
    {
        // GET: /Staff/Help — Help System
        public IActionResult Index() => View();

        // GET: /Staff/Help/Manual — Dina-download ang User Manual bilang tunay na PDF
        // file (attachment). Iisang PDF lamang ang ginagamit ng Admin at Staff.
        public IActionResult Manual()
            => File(UserManualPdf.GetBytes(), UserManualPdf.ContentType, UserManualPdf.FileName);
    }
}
