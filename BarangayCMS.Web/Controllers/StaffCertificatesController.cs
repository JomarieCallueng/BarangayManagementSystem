using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mammoth; // Siguraduhing na-install ito via PMC: Install-Package Mammoth
using BarangayCMS.DAL.Context;
using BarangayCMS.Entities;

namespace BarangayCMS.Web.Areas.Staff.Controllers
{
    [Area("Staff")]
    public class StaffCertificatesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StaffCertificatesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Unang Screen: Pipili si Staff ng Residente at Sertipiko
        public async Task<IActionResult> Index()
        {
            // Kukunin lang ang mga aktibong residente para hindi magulo ang listahan
            ViewBag.Residents = await _context.Residents
                .Where(r => r.IsResident)
                .ToListAsync();

            ViewBag.CertificateTypes = await _context.CertificateTypes.ToListAsync();
            return View();
        }

        // 2. I-proseso ang Template at Ipakita sa Screen para Ma-edit at Ma-print
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PreviewAndPrint(int residentId, int certificateTypeId)
        {
            var resident = await _context.Residents.FindAsync(residentId);
            var certType = await _context.CertificateTypes.FindAsync(certificateTypeId);

            if (resident == null || certType == null || string.IsNullOrEmpty(certType.TemplateFileName))
            {
                TempData["Error"] = "Hindi mahanap ang residente o walang template.";
                return RedirectToAction(nameof(Index));
            }

            // Path kung nasaan ang in-upload na Word file ni Admin
            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates", certType.TemplateFileName);

            if (!System.IO.File.Exists(templatePath))
            {
                TempData["Error"] = "Bura o wala sa server ang template file.";
                return RedirectToAction(nameof(Index));
            }

            // A. I-convert ang Word (.docx) papuntang HTML gamit ang Mammoth
            var converter = new DocumentConverter();
            var result = converter.ConvertToHtml(templatePath);
            string htmlContent = result.Value; // Ito na ang HTML version ng Word file

            // B. PAG-SAMA-SAMAHIN ANG MGA PANGALAN AT ADDRESS MULA SA IYONG ENTITY

            // Buong Pangalan (Halimbawa: Juan A. Dela Cruz Jr.)
            string middleInitial = !string.IsNullOrEmpty(resident.MiddleName) ? $"{resident.MiddleName[0]}." : "";
            string suffix = !string.IsNullOrEmpty(resident.Suffix) ? $" {resident.Suffix}" : "";
            string buongPangalan = $"{resident.FirstName} {middleInitial} {resident.LastName}{suffix}".Trim();

            // Kumpletong Address (Halimbawa: 123 Main St., Purok 4)
            string addressPart = $"{resident.HouseNumber} {resident.Street}".Trim();
            string sitioPart = !string.IsNullOrEmpty(resident.SitioPurok) ? $", {resident.SitioPurok}" : "";
            string kumpletongAddress = $"{addressPart}{sitioPart}".Trim();

            // Edad Computation base sa BirthDate
            int edad = DateTime.Now.Year - resident.BirthDate.Year;
            if (resident.BirthDate.Date > DateTime.Now.AddYears(-edad)) edad--; // Mas tumpak na edad

            // C. AUTOMATIC REPLACEMENT NG MGA {{placeholders}} SA LOOB NG WORD TEMPLATE
            htmlContent = htmlContent.Replace("{{pangalan}}", $"<strong>{buongPangalan}</strong>");
            htmlContent = htmlContent.Replace("{{tirahan}}", $"<strong>{kumpletongAddress}</strong>");
            htmlContent = htmlContent.Replace("{{edad}}", $"<strong>{edad}</strong>");
            htmlContent = htmlContent.Replace("{{petsa}}", $"<strong>{DateTime.Now.ToString("dd MMMM yyyy")}</strong>");

            // D. Ipasa ang nabuong HTML sa View
            ViewBag.HtmlContent = htmlContent;
            ViewBag.CertificateName = certType.CertificateName;

            return View();
        }
    }
}