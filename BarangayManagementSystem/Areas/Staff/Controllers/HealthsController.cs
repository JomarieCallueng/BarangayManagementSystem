using System;
using System.Collections.Generic;
using System.Linq;
using BarangayCMS.Areas.Staff.ViewModels;
using BarangayCMS.DAL.Context;
using BarangayCMS.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BarangayCMS.Areas.Staff.Controllers
{
    [Area("Staff")]
    public class HealthController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HealthController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Staff/Health
        public IActionResult Index()
        {
            var list = _context.HealthRecords
                .Include(h => h.Resident)
                .Select(h => new HealthViewModel
                {
                    Id = h.HealthRecordId, // Ito ang itinatawag ng asp-route-id="@item.Id" sa view mo
                    HealthRecordId = h.HealthRecordId,
                    ResidentId = h.ResidentId,
                    ResidentName = h.Resident != null
                        ? (h.Resident.FirstName + " " + (string.IsNullOrEmpty(h.Resident.MiddleName) ? "" : h.Resident.MiddleName + " ") + h.Resident.LastName + " " + h.Resident.Suffix).Trim()
                        : "Unknown Patient",
                    BloodType = h.BloodType,
                    HealthClassification = h.HealthClassification,
                    MedicalCondition = h.MedicalCondition,
                    IsVaccinated = h.IsVaccinated,
                    LastCheckupDate = h.LastCheckupDate,
                    Remarks = h.AttendingHealthWorker,
                    DateRecorded = h.DateLogged
                })
                .OrderByDescending(h => h.LastCheckupDate)
                .ToList();

            return View(list);
        }

        // 🛠️ BINAGO: Ginawang 'ViewRecord' mula sa 'Details' para tugma sa View History button mo!
        // GET: /Staff/Health/ViewRecord/5
        public IActionResult ViewRecord(int id)
        {
            var item = _context.HealthRecords
                .Include(h => h.Resident)
                .Where(h => h.HealthRecordId == id)
                .Select(h => new HealthViewModel
                {
                    Id = h.HealthRecordId,
                    HealthRecordId = h.HealthRecordId,
                    ResidentId = h.ResidentId,
                    ResidentName = h.Resident != null
                        ? (h.Resident.FirstName + " " + (string.IsNullOrEmpty(h.Resident.MiddleName) ? "" : h.Resident.MiddleName + " ") + h.Resident.LastName + " " + h.Resident.Suffix).Trim()
                        : "Unknown Patient",
                    BloodType = h.BloodType,
                    HealthClassification = h.HealthClassification,
                    MedicalCondition = h.MedicalCondition,
                    IsVaccinated = h.IsVaccinated,
                    LastCheckupDate = h.LastCheckupDate,
                    Remarks = h.AttendingHealthWorker,
                    DateRecorded = h.DateLogged
                })
                .FirstOrDefault();

            if (item == null) return NotFound();
            return View(item); // ⚠️ Tandaan: Dapat may View ka ring may pangalang 'ViewRecord.cshtml' sa folder mo.
        }

        // GET: /Staff/Health/Create
        public IActionResult Create()
        {
            var viewModel = new HealthViewModel
            {
                ResidentDataSource = GetResidentDropdownList()
            };
            return View(viewModel);
        }

        // POST: /Staff/Health/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(HealthViewModel model)
        {
            if (ModelState.IsValid)
            {
                var newRecord = new HealthRecord
                {
                    ResidentId = model.ResidentId,
                    MedicalCondition = model.MedicalCondition,
                    BloodType = model.BloodType ?? "N/A",
                    HealthClassification = model.HealthClassification ?? "General",
                    IsVaccinated = model.IsVaccinated,
                    LastCheckupDate = model.LastCheckupDate,
                    AttendingHealthWorker = model.Remarks ?? "Duty Nurse",
                    DateLogged = DateTime.Now,
                    WeightKg = 0,
                    HeightCm = 0
                };

                _context.HealthRecords.Add(newRecord);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            model.ResidentDataSource = GetResidentDropdownList();
            return View(model);
        }

        // GET: /Staff/Health/Edit/5
        public IActionResult Edit(int id)
        {
            var item = _context.HealthRecords.FirstOrDefault(h => h.HealthRecordId == id);
            if (item == null) return NotFound();

            var viewModel = new HealthViewModel
            {
                Id = item.HealthRecordId,
                HealthRecordId = item.HealthRecordId,
                ResidentId = item.ResidentId,
                MedicalCondition = item.MedicalCondition,
                BloodType = item.BloodType,
                HealthClassification = item.HealthClassification,
                IsVaccinated = item.IsVaccinated,
                LastCheckupDate = item.LastCheckupDate,
                Remarks = item.AttendingHealthWorker,
                DateRecorded = item.DateLogged,
                ResidentDataSource = GetResidentDropdownList()
            };

            return View(viewModel);
        }

        // POST: /Staff/Health/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, HealthViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existing = _context.HealthRecords.FirstOrDefault(h => h.HealthRecordId == id);
                if (existing == null) return NotFound();

                existing.ResidentId = model.ResidentId;
                existing.MedicalCondition = model.MedicalCondition;
                existing.BloodType = model.BloodType;
                existing.HealthClassification = model.HealthClassification;
                existing.IsVaccinated = model.IsVaccinated;
                existing.LastCheckupDate = model.LastCheckupDate;
                existing.AttendingHealthWorker = model.Remarks;

                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            model.ResidentDataSource = GetResidentDropdownList();
            return View(model);
        }

        // GET: /Staff/Health/Delete/5
        public IActionResult Delete(int id)
        {
            var item = _context.HealthRecords
                .Include(h => h.Resident)
                .Where(h => h.HealthRecordId == id)
                .Select(h => new HealthViewModel
                {
                    Id = h.HealthRecordId,
                    HealthRecordId = h.HealthRecordId,
                    MedicalCondition = h.MedicalCondition,
                    ResidentName = h.Resident != null
                        ? (h.Resident.FirstName + " " + h.Resident.LastName)
                        : "Unknown Patient"
                })
                .FirstOrDefault();

            if (item == null) return NotFound();
            return View(item);
        }

        // POST: /Staff/Health/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var item = _context.HealthRecords.FirstOrDefault(h => h.HealthRecordId == id);
            if (item != null)
            {
                _context.HealthRecords.Remove(item);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }

        private List<SelectListItem> GetResidentDropdownList()
        {
            return _context.Residents
                .Where(r => r.IsResident)
                .Select(r => new SelectListItem
                {
                    Value = r.ResidentId.ToString(),
                    Text = (r.FirstName + " " + (string.IsNullOrEmpty(r.MiddleName) ? "" : r.MiddleName + " ") + r.LastName + " " + r.Suffix).Trim()
                })
                .ToList();
        }
    }
}