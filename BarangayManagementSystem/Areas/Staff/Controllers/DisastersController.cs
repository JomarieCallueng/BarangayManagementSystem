using Microsoft.AspNetCore.Mvc;
using BarangayCMS.Areas.Staff.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BarangayCMS.Areas.Staff.Controllers
{
    [Area("Staff")]
    public class DisastersController : Controller
    {
        // In-update na Mock DB engine gamit ang bagong properties para sa magandang UI table mo
        private static List<DisasterViewModel> _disasters = new List<DisasterViewModel>
        {
            new DisasterViewModel
            {
                Id = 1,
                IncidentName = "Super Typhoon Egay",
                DisasterType = "Typhoon Flooding",
                Description = "Baha hanggang tuhod sa mababang bahagi ng Purok 3 dahil sa walang humpay na ulan.",
                Location = "Purok 3 (Riverside)",
                OccurrenceDate = DateTime.Now.AddDays(-1),
                AffectedHouseholdsCount = 45,
                DisplacedIndividualsCount = 180,
                EvacuationCenterStatus = "Open",
                ReliefDistributionStatus = "Ongoing",
                Status = "Active"
            },
            new DisasterViewModel
            {
                Id = 2,
                IncidentName = "Purok 5 Fire Incident",
                DisasterType = "Residential Fire",
                Description = "Naapula na ang sunog sa isang residential structure. Walang naiulat na sugatan.",
                Location = "Purok 5, J.P. Rizal St.",
                OccurrenceDate = DateTime.Now.AddDays(-3),
                AffectedHouseholdsCount = 12,
                DisplacedIndividualsCount = 48,
                EvacuationCenterStatus = "Closed",
                ReliefDistributionStatus = "Completed",
                Status = "Resolved"
            }
        };

        // GET: /Staff/Disasters/Index
        public IActionResult Index()
        {
            var list = _disasters.OrderByDescending(d => d.OccurrenceDate).ToList();
            return View(list);
        }

        // GET: /Staff/Disasters/Manage/5 (Ito ang tinatawag ng iyong 'Track' button!)
        public IActionResult Manage(int id)
        {
            var item = _disasters.FirstOrDefault(d => d.Id == id);
            if (item == null) return NotFound();
            return View(item); // Siguraduhing may Manage.cshtml ka o palitan mo ito ng Redirect/Details kung wala pa.
        }

        // GET: /Staff/Disasters/Details/5
        public IActionResult Details(int id)
        {
            var item = _disasters.FirstOrDefault(d => d.Id == id);
            if (item == null) return NotFound();
            return View(item);
        }

        // GET: /Staff/Disasters/Create
        public IActionResult Create()
        {
            return View(new DisasterViewModel());
        }

        // POST: /Staff/Disasters/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(DisasterViewModel model)
        {
            if (ModelState.IsValid)
            {
                model.Id = _disasters.Count > 0 ? _disasters.Max(d => d.Id) + 1 : 1;
                _disasters.Add(model);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: /Staff/Disasters/Edit/5
        public IActionResult Edit(int id)
        {
            var item = _disasters.FirstOrDefault(d => d.Id == id);
            if (item == null) return NotFound();
            return View(item);
        }

        // POST: /Staff/Disasters/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, DisasterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existing = _disasters.FirstOrDefault(d => d.Id == id);
                if (existing == null) return NotFound();

                existing.IncidentName = model.IncidentName;
                existing.DisasterType = model.DisasterType;
                existing.Description = model.Description;
                existing.Location = model.Location;
                existing.OccurrenceDate = model.OccurrenceDate;
                existing.AffectedHouseholdsCount = model.AffectedHouseholdsCount;
                existing.DisplacedIndividualsCount = model.DisplacedIndividualsCount;
                existing.EvacuationCenterStatus = model.EvacuationCenterStatus;
                existing.ReliefDistributionStatus = model.ReliefDistributionStatus;
                existing.Status = model.Status;

                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: /Staff/Disasters/Delete/5
        public IActionResult Delete(int id)
        {
            var item = _disasters.FirstOrDefault(d => d.Id == id);
            if (item == null) return NotFound();
            return View(item);
        }

        // POST: /Staff/Disasters/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var item = _disasters.FirstOrDefault(d => d.Id == id);
            if (item != null)
            {
                _disasters.Remove(item);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}