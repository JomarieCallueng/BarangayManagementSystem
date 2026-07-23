using Microsoft.AspNetCore.Mvc;
using BarangayCMS.Entities;
using BarangayCMS.DAL.Context;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace BarangayCMS.Web.Controllers
{
    // 💡 Ito ang magic route para gumana ang https://localhost:7268/Complaint/
    [Route("Complaint")]
    public class ComplaintController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ComplaintController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🌟 GET: /Complaint/Categories (Para sa error mo kanina sa screenshot)
        [HttpGet("Categories")]
        public IActionResult Categories()
        {
            return View();
        }

        // 🌟 GET: /Complaint/Create
        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View();
        }

        // 🌟 POST: /Complaint/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string ComplainantName, string ComplainantContact, string IncidentLocation, DateTime IncidentDate, string Details)
        {
            if (string.IsNullOrEmpty(ComplainantName) || string.IsNullOrEmpty(Details))
            {
                ModelState.AddModelError(string.Empty, "Mangyaring punan ang mga kinakailangang impormasyon.");
                return View();
            }

            try
            {
                // 🔍 MAGHANAP NG VALID RESIDENT ID PARA SA FOREIGN KEY
                var defaultResident = await _context.Residents.FirstOrDefaultAsync();

                if (defaultResident == null)
                {
                    ModelState.AddModelError(string.Empty, "Hindi pwedeng mag-save ng reklamo dahil walang laman ang 'Residents' table mo. Mag-add ka muna ng kahit isang Residente sa Admin portal.");
                    return View();
                }

                string generatedCaseNumber = "CMP-" + DateTime.Now.ToString("yyyyMMdd") + "-" + new Random().Next(100, 999);

                var complaint = new Complaint
                {
                    CaseNumber = generatedCaseNumber,
                    ComplainantName = ComplainantName,
                    ComplainantContact = ComplainantContact,
                    IncidentLocation = IncidentLocation,
                    IncidentDate = IncidentDate,
                    Details = Details,

                    Status = "Pending",
                    RespondentName = "N/A",
                    Remarks = "Submitted via Public Web Portal",
                    DateSubmitted = DateTime.Now,

                    // 🔑 IPALIT ANG TUNAY AT VALID NA RESIDENT ID
                    ResidentId = defaultResident.ResidentId
                };

                _context.Complaints.Add(complaint);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Matagumpay na naipadala ang inyong reklamo!";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                ModelState.AddModelError(string.Empty, "Nagkaroon ng problema sa pag-save: " + innerMessage);
                return View();
            }
        }
    }
}