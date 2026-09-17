using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BarangayCMS.BLL.Interfaces;
using BarangayCMS.DTO;

namespace BarangayManagementSystem.Controllers
{
    /// <summary>
    /// Pampublikong (read-only) na Evacuation Information page. Ang publiko/
    /// residente ay TUMITINGIN lamang — walang create/edit/delete dito. Lahat
    /// ng datos ay galing sa database sa pamamagitan ng IEvacuationService
    /// (parehong pinanggagalingan ng Admin/Staff — walang hardcoded na listahan).
    /// </summary>
    public class EvacuationController : Controller
    {
        private readonly IEvacuationService _evacuationService;

        public EvacuationController(IEvacuationService evacuationService)
        {
            _evacuationService = evacuationService;
        }

        // GET: /Evacuation
        public async Task<IActionResult> Index()
        {
            try
            {
                var model = await _evacuationService.GetPublicEvacuationInfoAsync();
                return View(model);
            }
            catch (Exception)
            {
                // Hindi ipinapakita sa publiko ang raw error/stack trace — nagse-set
                // lang ng flag para maipakita ang malinis na error state sa view.
                ViewBag.LoadFailed = true;
                return View(new PublicEvacuationDTO());
            }
        }
    }
}
