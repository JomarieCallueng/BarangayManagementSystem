using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BarangayCMS.DAL.Context;
using BarangayCMS.Entities;
using BarangayCMS.Web.Areas.Admin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BarangayCMS.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ComplaintsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ComplaintsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. GET: Admin/Complaints
        public async Task<IActionResult> Index()
        {
            var complaints = await _context.Complaints
                .Include(c => c.Resident)
                .Select(c => new ComplaintViewModel
                {
                    Id = c.ComplaintId,
                    ResidentId = c.ResidentId,
                    // CHANGER: Kung may laman ang ComplainantName (mula sa public form), ito ang ipakita. Kung wala, fallback sa Resident name.
                    ResidentFullName = !string.IsNullOrEmpty(c.ComplainantName) ? c.ComplainantName :
                                       (c.Resident != null ? $"{c.Resident.LastName}, {c.Resident.FirstName}" : "Unknown Resident"),
                    Subject = !string.IsNullOrEmpty(c.CaseNumber) ? c.CaseNumber : "No Case Number",
                    Description = c.Details,
                    DateSubmitted = c.DateSubmitted,
                    Status = c.Status
                }).ToListAsync();

            return View(complaints);
        }

        // 2. GET: Admin/Complaints/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var complaint = await _context.Complaints
                .Include(c => c.Resident)
                .FirstOrDefaultAsync(m => m.ComplaintId == id);

            if (complaint == null) return NotFound();

            var viewModel = new ComplaintViewModel
            {
                Id = complaint.ComplaintId,
                ResidentId = complaint.ResidentId,
                // CHANGER: Ginagamit na ang nakasulat na pangalan para sa public submissions upang mawala si Jomarie
                ResidentFullName = !string.IsNullOrEmpty(complaint.ComplainantName) ? complaint.ComplainantName :
                                   (complaint.Resident != null ? $"{complaint.Resident.LastName}, {complaint.Resident.FirstName} {complaint.Resident.MiddleName}".Trim() : "Unknown Resident"),
                Subject = !string.IsNullOrEmpty(complaint.CaseNumber) ? complaint.CaseNumber : "No Case Number",
                Description = complaint.Details,
                DateSubmitted = complaint.DateSubmitted,
                Status = complaint.Status
            };

            return View(viewModel);
        }

        // 3. GET: Admin/Complaints/Create
        public async Task<IActionResult> Create()
        {
            var viewModel = new ComplaintViewModel
            {
                DateSubmitted = DateTime.Now,
                ResidentList = await GetResidentSelectList()
            };
            return View(viewModel);
        }

        // 4. POST: Admin/Complaints/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ComplaintViewModel model)
        {
            if (ModelState.IsValid)
            {
                var resident = await _context.Residents.FindAsync(model.ResidentId);
                string residentName = resident != null ? $"{resident.LastName}, {resident.FirstName}" : "Unknown";
                string residentContact = resident?.ContactNumber ?? string.Empty;

                var complaint = new Complaint
                {
                    ResidentId = model.ResidentId,
                    Details = model.Description ?? string.Empty,
                    DateSubmitted = model.DateSubmitted,
                    Status = model.Status ?? "Pending",

                    CaseNumber = $"CMP-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString()[..4].ToUpper()}",
                    ComplainantName = residentName,
                    ComplainantContact = residentContact,
                    RespondentName = "To Be Determined",
                    IncidentLocation = resident?.Street ?? "Barangay Hall",
                    IncidentDate = model.DateSubmitted,
                    Remarks = string.Empty,
                    ActionTaken = string.Empty
                };

                _context.Complaints.Add(complaint);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            model.ResidentList = await GetResidentSelectList();
            return View(model);
        }

        // 5. GET: Admin/Complaints/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var complaint = await _context.Complaints
                .Include(c => c.Resident)
                .FirstOrDefaultAsync(m => m.ComplaintId == id);

            if (complaint == null) return NotFound();

            var viewModel = new ComplaintViewModel
            {
                Id = complaint.ComplaintId,
                ResidentId = complaint.ResidentId,
                // CHANGER: Ipinapasa ang tamang pangalan sa model para sa rendering reference
                ResidentFullName = !string.IsNullOrEmpty(complaint.ComplainantName) ? complaint.ComplainantName :
                                   (complaint.Resident != null ? $"{complaint.Resident.LastName}, {complaint.Resident.FirstName}" : "Unknown Resident"),
                Subject = !string.IsNullOrEmpty(complaint.CaseNumber) ? complaint.CaseNumber : "No Case Number",
                Description = complaint.Details,
                DateSubmitted = complaint.DateSubmitted,
                Status = complaint.Status,
                ResidentList = await GetResidentSelectList()
            };

            return View(viewModel);
        }

        // 6. POST: Admin/Complaints/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ComplaintViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var complaint = await _context.Complaints.FindAsync(id);
                    if (complaint == null) return NotFound();

                    // Kung ito ay galing sa public web panel, pinapanatili ang orihinal na ComplainantName link
                    if (complaint.CaseNumber == null || !complaint.CaseNumber.StartsWith("CMP"))
                    {
                        complaint.ResidentId = model.ResidentId;
                        var resident = await _context.Residents.FindAsync(model.ResidentId);
                        if (resident != null)
                        {
                            complaint.ComplainantName = $"{resident.LastName}, {resident.FirstName}";
                        }
                    }

                    complaint.Details = model.Description ?? string.Empty;
                    complaint.DateSubmitted = model.DateSubmitted;
                    complaint.Status = model.Status;

                    _context.Complaints.Update(complaint);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Complaints.Any(e => e.ComplaintId == model.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            model.ResidentList = await GetResidentSelectList();
            return View(model);
        }

        // 7. GET: Admin/Complaints/Assign/5
        public async Task<IActionResult> Assign(int? id)
        {
            if (id == null) return NotFound();

            var complaint = await _context.Complaints
                .Include(c => c.Resident)
                .FirstOrDefaultAsync(m => m.ComplaintId == id);

            if (complaint == null) return NotFound();

            var viewModel = new ComplaintViewModel
            {
                Id = complaint.ComplaintId,
                // CHANGER: Pinalitan para ipakita ang totoong nagreklamo sa dispatch screen
                ResidentFullName = !string.IsNullOrEmpty(complaint.ComplainantName) ? complaint.ComplainantName :
                                   (complaint.Resident != null ? $"{complaint.Resident.LastName}, {complaint.Resident.FirstName}" : "Unknown Resident"),
                Subject = !string.IsNullOrEmpty(complaint.CaseNumber) ? complaint.CaseNumber : "No Case Number",
                Description = complaint.Details,
                Status = complaint.Status
            };

            return View(viewModel);
        }

        // 8. POST: Admin/Complaints/Assign/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(int id, ComplaintViewModel model)
        {
            var complaint = await _context.Complaints.FindAsync(id);
            if (complaint == null) return NotFound();

            complaint.Status = "Assigned";
            complaint.ActionTaken = $"Assigned to Barangay Lupon Officer on {DateTime.Now:yyyy-MM-dd}";

            _context.Complaints.Update(complaint);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // 9. GET: Admin/Complaints/UpdateStatus/5
        public async Task<IActionResult> UpdateStatus(int? id)
        {
            if (id == null) return NotFound();

            var complaint = await _context.Complaints
                .Include(c => c.Resident)
                .FirstOrDefaultAsync(m => m.ComplaintId == id);

            if (complaint == null) return NotFound();

            var viewModel = new ComplaintViewModel
            {
                Id = complaint.ComplaintId,
                // CHANGER: Inaayos din dito para hindi na sumingit si Jomarie sa modal/status update state
                ResidentFullName = !string.IsNullOrEmpty(complaint.ComplainantName) ? complaint.ComplainantName :
                                   (complaint.Resident != null ? $"{complaint.Resident.LastName}, {complaint.Resident.FirstName}" : "Unknown Resident"),
                Subject = !string.IsNullOrEmpty(complaint.CaseNumber) ? complaint.CaseNumber : "No Case Number",
                Status = complaint.Status
            };

            return View(viewModel);
        }

        // 10. POST: Admin/Complaints/UpdateStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var complaint = await _context.Complaints.FindAsync(id);
            if (complaint == null) return NotFound();

            complaint.Status = status;
            complaint.Remarks = $"Status dynamically changed to '{status}' via Admin Panel.";

            _context.Complaints.Update(complaint);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // 11. GET: Admin/Complaints/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var complaint = await _context.Complaints
                .Include(c => c.Resident)
                .FirstOrDefaultAsync(m => m.ComplaintId == id);

            if (complaint == null) return NotFound();

            var viewModel = new ComplaintViewModel
            {
                Id = complaint.ComplaintId,
                // CHANGER: Pinatitindi ang seguridad ng deletion panel sa pagpapakita ng tunay na sumulat ng reklamo
                ResidentFullName = !string.IsNullOrEmpty(complaint.ComplainantName) ? complaint.ComplainantName :
                                   (complaint.Resident != null ? $"{complaint.Resident.LastName}, {complaint.Resident.FirstName}" : "Unknown Resident"),
                Subject = !string.IsNullOrEmpty(complaint.CaseNumber) ? complaint.CaseNumber : "No Case Number",
                Status = complaint.Status
            };

            return View(viewModel);
        }

        // 12. POST: Admin/Complaints/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var complaint = await _context.Complaints.FindAsync(id);
            if (complaint != null)
            {
                _context.Complaints.Remove(complaint);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task<IEnumerable<SelectListItem>> GetResidentSelectList()
        {
            return await _context.Residents
                .OrderBy(r => r.LastName)
                .Select(r => new SelectListItem
                {
                    Value = r.ResidentId.ToString(),
                    Text = $"{r.LastName}, {r.FirstName}"
                }).ToListAsync();
        }
    }
}