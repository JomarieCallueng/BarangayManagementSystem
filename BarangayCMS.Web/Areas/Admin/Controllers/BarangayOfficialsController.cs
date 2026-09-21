using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BarangayCMS.DAL.Context;
using BarangayCMS.Entities;
using BarangayCMS.Web.Areas.Admin.Models;
using BarangayCMS.Web.Areas.Admin.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace BarangayCMS.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BarangayOfficialsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BarangayOfficialsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // 1. GET: Admin/BarangayOfficials
        public async Task<IActionResult> Index()
        {
            var officials = await _context.BarangayOfficials
                .OrderBy(o => o.Position == "Barangay Captain" ? 0 : 1)
                .ThenBy(o => o.BarangayOfficialId)
                .Select(o => new BarangayOfficialViewModel
                {
                    Id = o.BarangayOfficialId,
                    FullName = o.FullName,
                    Position = o.Position,
                    Committee = o.Committee,
                    SignaturePath = o.SignaturePath,
                    ProfileImagePath = o.ProfileImagePath,
                    IsActive = o.IsActive
                }).ToListAsync();

            // Structure summary (Captain: 1/1, Kagawad: x/7, ...) mula sa aktibong records.
            var activePositions = officials.Where(o => o.IsActive).Select(o => o.Position).ToList();
            ViewBag.Summary = OfficialStructure.BuildSummary(activePositions);

            return View(officials);
        }

        // 2. GET: Admin/BarangayOfficials/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var official = await _context.BarangayOfficials
                .FirstOrDefaultAsync(m => m.BarangayOfficialId == id);

            if (official == null) return NotFound();

            return View(ToViewModel(official));
        }

        // 3. GET: Admin/BarangayOfficials/Create
        public async Task<IActionResult> Create()
        {
            await PopulateAvailabilityAsync(0);
            return View(new BarangayOfficialViewModel { IsActive = true });
        }

        // 4. POST: Admin/BarangayOfficials/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BarangayOfficialViewModel model)
        {
            // Server-side validation = panghuling awtoridad: duplicate active name,
            // slot limits, at committee uniqueness bawat konseho.
            await ValidateOfficialAsync(model, excludeId: 0);

            if (ModelState.IsValid)
            {
                var official = new BarangayOfficial
                {
                    FullName = model.FullName,
                    Position = model.Position,
                    // Role-based committee (server-side final authority).
                    Committee = OfficialStructure.NormalizeCommittee(model.Position, model.Committee),
                    // E-Signature inalis na sa Officials UI; walang bagong signature dito.
                    SignaturePath = string.Empty,
                    ProfileImagePath = await SaveUploadAsync(model.ProfileImageFile, "officials"),
                    IsActive = model.IsActive
                };

                _context.BarangayOfficials.Add(official);
                await _context.SaveChangesAsync();

                // Awtomatikong itala ang kasalukuyang termino sa service history ng taong ito
                // para agad may laman ang public na "Official History" section.
                _context.OfficialServiceHistories.Add(new OfficialServiceHistory
                {
                    BarangayOfficialId = official.BarangayOfficialId,
                    Position = official.Position,
                    Committee = official.Committee,
                    StartDate = DateTime.Now,
                    EndDate = null,
                    Status = "Current"
                });
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Official \"{official.FullName}\" has been added successfully.";
                return RedirectToAction(nameof(Index));
            }
            await PopulateAvailabilityAsync(0);
            return View(model);
        }

        // 5. GET: Admin/BarangayOfficials/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var official = await _context.BarangayOfficials.FindAsync(id.Value);
            if (official == null) return NotFound();

            await PopulateAvailabilityAsync(id.Value);
            return View(ToViewModel(official));
        }

        // 6. POST: Admin/BarangayOfficials/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BarangayOfficialViewModel model)
        {
            if (id != model.Id) return NotFound();

            // Server-side validation — hindi kasama ang record na ineedit sa bilang.
            await ValidateOfficialAsync(model, excludeId: id);

            if (ModelState.IsValid)
            {
                try
                {
                    var official = await _context.BarangayOfficials.FindAsync(id);
                    if (official == null) return NotFound();

                    // E-Signature inalis na sa Officials UI — pinananatili ang dating
                    // SignaturePath sa database (hindi binabago dito).

                    // Profile photo: palitan lang kung may bagong in-upload.
                    if (model.ProfileImageFile != null && model.ProfileImageFile.Length > 0)
                    {
                        DeleteUpload(official.ProfileImagePath, "officials");
                        official.ProfileImagePath = await SaveUploadAsync(model.ProfileImageFile, "officials");
                    }

                    official.FullName = model.FullName;
                    official.Position = model.Position;
                    // I-recalculate ang committee base sa (posibleng bago) na posisyon:
                    // linisin para sa Captain/Sec/Treas, sapilitan para sa SK Chair, atbp.
                    official.Committee = OfficialStructure.NormalizeCommittee(model.Position, model.Committee);
                    official.IsActive = model.IsActive;

                    _context.BarangayOfficials.Update(official);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.BarangayOfficials.Any(e => e.BarangayOfficialId == model.Id)) return NotFound();
                    else throw;
                }
                TempData["SuccessMessage"] = $"Official \"{model.FullName}\" has been updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            await PopulateAvailabilityAsync(id);
            return View(model);
        }

        // 7. GET: Admin/BarangayOfficials/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var official = await _context.BarangayOfficials
                .FirstOrDefaultAsync(m => m.BarangayOfficialId == id);

            if (official == null) return NotFound();

            return View(ToViewModel(official));
        }

        // 8. POST: Admin/BarangayOfficials/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var official = await _context.BarangayOfficials.FindAsync(id);
            if (official != null)
            {
                // Burahin ang mga naka-upload na file (signature + profile photo).
                DeleteUpload(official.SignaturePath, "signatures");
                DeleteUpload(official.ProfileImagePath, "officials");

                // Ang service history ay cascade-delete na kasama ng opisyal.
                _context.BarangayOfficials.Remove(official);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Official \"{official.FullName}\" has been removed.";
            }
            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // SERVICE HISTORY MANAGEMENT (per official)
        // ============================================================

        // 9. GET: Admin/BarangayOfficials/History/5
        public async Task<IActionResult> History(int? id)
        {
            if (id == null) return NotFound();

            var official = await _context.BarangayOfficials
                .Include(o => o.ServiceHistories)
                .FirstOrDefaultAsync(o => o.BarangayOfficialId == id);
            if (official == null) return NotFound();

            return View(BuildHistoryViewModel(official));
        }

        // 10. POST: Admin/BarangayOfficials/AddHistory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddHistory(OfficialHistoryViewModel model)
        {
            var official = await _context.BarangayOfficials
                .Include(o => o.ServiceHistories)
                .FirstOrDefaultAsync(o => o.BarangayOfficialId == model.OfficialId);
            if (official == null) return NotFound();

            // Basic year-order feedback (also enforced sa ValidateServiceTerm).
            if (model.NewEndYear.HasValue && model.NewStartYear.HasValue && model.NewEndYear < model.NewStartYear)
                ModelState.AddModelError("NewEndYear", "Start Year cannot be later than End Year.");

            if (!ModelState.IsValid)
                return HistoryWithInput(official, model);

            var pos = model.NewPosition.Trim();
            var com = (model.NewCommittee ?? string.Empty).Trim();
            int start = model.NewStartYear!.Value;
            int? end = model.NewEndYear;

            var allTerms = official.ServiceHistories.ToList();

            // ============================================================
            // NEW *PRESENT* (open-ended) TERM  →  reelection / consecutive
            // ------------------------------------------------------------
            // Rule: only ONE Present term may exist per person. Adding a new
            // Present term automatically CLOSES the previous open term(s) at
            // the new Start Year (2023–Present + new 2026  ⇒  2023–2026 +
            // 2026–Present). This is NOT a duplicate — it is reelection /
            // consecutive service. Closing the old term and creating the new
            // one happen inside ONE transaction (data safety).
            // ============================================================
            if (!end.HasValue)
            {
                var openTerms = allTerms.Where(t => t.EndDate == null).ToList();

                // A new current term must begin AFTER every existing open term.
                // Starting before/at the current term is a genuine overlap, not
                // a valid consecutive term, so it is rejected.
                var blocker = openTerms.FirstOrDefault(t => t.StartDate.Year >= start);
                if (blocker != null)
                {
                    ModelState.AddModelError("NewStartYear",
                        $"A new current term must start after the existing current term ({blocker.StartDate.Year}–Present).");
                    return HistoryWithInput(official, model);
                }

                // Validate the new Present term against the person's COMPLETED
                // terms only. The open term(s) are excluded because they will be
                // closed here — this is exactly what lets reelection through
                // instead of failing the "already has a current term" rule.
                var completed = allTerms.Where(t => t.EndDate != null).ToList();
                var openErr = ValidateServiceTerm(completed, pos, com, start, end);
                if (openErr != null)
                {
                    ModelState.AddModelError(string.Empty, openErr);
                    return HistoryWithInput(official, model);
                }

                using var tx = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Close the previous open term(s) using the new Start Year.
                    // Looping keeps a normal single-open term correct AND lets any
                    // legacy multi-open (corrupt) data self-heal to one Present.
                    foreach (var openTerm in openTerms)
                    {
                        openTerm.EndDate = new DateTime(start, 12, 31);
                        openTerm.Status = "Completed";
                    }

                    _context.OfficialServiceHistories.Add(new OfficialServiceHistory
                    {
                        BarangayOfficialId = official.BarangayOfficialId,
                        Position = pos,
                        Committee = com,
                        StartDate = new DateTime(start, 1, 1),
                        EndDate = null,
                        Status = "Current"
                    });

                    await _context.SaveChangesAsync();
                    await tx.CommitAsync();
                }
                catch
                {
                    // If creation fails: never leave the old term incorrectly
                    // closed and never create a partial/duplicate record.
                    await tx.RollbackAsync();
                    throw;
                }

                TempData["SuccessMessage"] = openTerms.Any()
                    ? "Reelection recorded: previous term closed and new current term created."
                    : "Naidagdag ang kasalukuyang termino sa service history.";
                return RedirectToAction(nameof(History), new { id = official.BarangayOfficialId });
            }

            // ============================================================
            // NEW *COMPLETED* TERM (End Year supplied)  →  historical record
            // ============================================================
            var err = ValidateServiceTerm(allTerms, pos, com, start, end);
            if (err != null)
            {
                ModelState.AddModelError(string.Empty, err);
                return HistoryWithInput(official, model);
            }

            _context.OfficialServiceHistories.Add(new OfficialServiceHistory
            {
                BarangayOfficialId = official.BarangayOfficialId,
                Position = pos,
                Committee = com,
                StartDate = new DateTime(start, 1, 1),
                EndDate = new DateTime(end.Value, 12, 31),
                Status = "Completed"
            });

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Naidagdag ang termino sa service history.";
            return RedirectToAction(nameof(History), new { id = official.BarangayOfficialId });
        }

        // 11b. POST: Admin/BarangayOfficials/EditHistory — ina-update ang UMIIRAL na
        // service-history record (hindi gumagawa ng bago; pinananatili ang Id at ang
        // ugnayan sa opisyal). Server-side ang validation.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditHistory(int historyId, int officialId, string editPosition,
            string? editCommittee, int? editStartYear, int? editEndYear)
        {
            var term = await _context.OfficialServiceHistories
                .FirstOrDefaultAsync(h => h.Id == historyId && h.BarangayOfficialId == officialId);
            if (term == null) return NotFound();

            // --- Basic required/range checks ---
            if (string.IsNullOrWhiteSpace(editPosition))
            {
                TempData["ErrorMessage"] = "Position is required.";
                return RedirectToAction(nameof(History), new { id = officialId });
            }
            if (!editStartYear.HasValue || editStartYear < 1900 || editStartYear > 2200)
            {
                TempData["ErrorMessage"] = "A valid Start Year is required.";
                return RedirectToAction(nameof(History), new { id = officialId });
            }
            if (editEndYear.HasValue && (editEndYear < 1900 || editEndYear > 2200))
            {
                TempData["ErrorMessage"] = "Invalid End Year.";
                return RedirectToAction(nameof(History), new { id = officialId });
            }

            // --- Comprehensive validation vs OTHER terms (exclude this record's own Id) ---
            var others = await _context.OfficialServiceHistories
                .Where(h => h.BarangayOfficialId == officialId && h.Id != historyId)
                .ToListAsync();

            var err = ValidateServiceTerm(others, editPosition.Trim(), (editCommittee ?? string.Empty).Trim(),
                editStartYear.Value, editEndYear);
            if (err != null)
            {
                TempData["ErrorMessage"] = err;
                return RedirectToAction(nameof(History), new { id = officialId });
            }

            // --- I-update ang PAREHONG record (preserve Id + relationship) ---
            term.Position = editPosition.Trim();
            term.Committee = (editCommittee ?? string.Empty).Trim();
            term.StartDate = new DateTime(editStartYear.Value, 1, 1);
            term.EndDate = editEndYear.HasValue ? new DateTime(editEndYear.Value, 12, 31) : (DateTime?)null;
            term.Status = editEndYear.HasValue ? "Completed" : "Current";

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Na-update ang termino sa service history.";
            return RedirectToAction(nameof(History), new { id = officialId });
        }

        // 11. POST: Admin/BarangayOfficials/DeleteHistory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteHistory(int historyId, int officialId)
        {
            var term = await _context.OfficialServiceHistories
                .FirstOrDefaultAsync(h => h.Id == historyId && h.BarangayOfficialId == officialId);
            if (term != null)
            {
                _context.OfficialServiceHistories.Remove(term);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Natanggal ang termino sa service history.";
            }
            return RedirectToAction(nameof(History), new { id = officialId });
        }

        // ============================================================
        // HELPERS
        // ============================================================

        // Snapshot ng mga aktibong opisyal (hindi kasama ang isang id sa Edit).
        private async Task<List<(string FullName, string Position, string Committee)>> ActiveSnapshotAsync(int excludeId)
        {
            var rows = await _context.BarangayOfficials
                .Where(o => o.IsActive && o.BarangayOfficialId != excludeId)
                .Select(o => new { o.FullName, o.Position, o.Committee })
                .ToListAsync();
            return rows.Select(r => (r.FullName, r.Position, r.Committee ?? string.Empty)).ToList();
        }

        // Mga committee na kasalukuyang ginagamit ng aktibong Kagawad ng PAREHONG konseho
        // (SB o SK) — para sa committee-uniqueness rule (hiwalay ang pools ng SB at SK).
        private static List<string> UsedCommittees(IEnumerable<(string FullName, string Position, string Committee)> actives, OfficialSlot kagawadSlot)
            => actives
                .Where(a => OfficialStructure.Classify(a.Position) == kagawadSlot && !string.IsNullOrWhiteSpace(a.Committee))
                .Select(a => a.Committee.Trim())
                .ToList();

        // Sentralisadong server-side validation para sa Create at Edit.
        private async Task ValidateOfficialAsync(BarangayOfficialViewModel model, int excludeId)
        {
            // Inaktibong record ay hindi umuubos ng slot/name/committee — active lang.
            if (!model.IsActive) return;

            var actives = await ActiveSnapshotAsync(excludeId);

            // (2) Duplicate ACTIVE name (normalized: trim, collapse spaces, case-insensitive).
            var norm = OfficialStructure.NormalizeName(model.FullName);
            if (actives.Any(a => OfficialStructure.NormalizeName(a.FullName) == norm))
                ModelState.AddModelError(nameof(model.FullName), "An active official with this name already exists.");

            // (10) Slot limits.
            var slotError = OfficialStructure.ValidateSlot(model.Position, actives.Select(a => a.Position));
            if (slotError != null)
                ModelState.AddModelError(nameof(model.Position), slotError);

            // (3) Committee uniqueness — Kagawad lamang, hiwalay ang SB at SK pools.
            var slot = OfficialStructure.Classify(model.Position);
            if (OfficialStructure.IsKagawadSlot(slot))
            {
                var committee = (model.Committee ?? string.Empty).Trim();
                if (!string.IsNullOrEmpty(committee))
                {
                    var used = UsedCommittees(actives, slot);
                    if (used.Any(c => string.Equals(c, committee, System.StringComparison.OrdinalIgnoreCase)))
                    {
                        var council = slot == OfficialSlot.SkKagawad ? "SK" : "Barangay";
                        ModelState.AddModelError(nameof(model.Committee),
                            $"This committee is already assigned to another active {council} Kagawad.");
                    }
                }
            }
        }

        // Naglalagay sa ViewBag ng data para sa Create/Edit forms: slot usage, summary,
        // aktibong pangalan (dup-name JS) at ginagamit na committees (disable sa selector).
        private async Task PopulateAvailabilityAsync(int excludeId)
        {
            var actives = await ActiveSnapshotAsync(excludeId);
            var positions = actives.Select(a => a.Position).ToList();

            ViewBag.SlotCounts = OfficialStructure.CountBySlot(positions);
            ViewBag.Summary = OfficialStructure.BuildSummary(positions);
            ViewBag.ActiveNames = actives.Select(a => OfficialStructure.NormalizeName(a.FullName)).ToList();
            ViewBag.UsedBarangayCommittees = UsedCommittees(actives, OfficialSlot.BarangayKagawad);
            ViewBag.UsedSkCommittees = UsedCommittees(actives, OfficialSlot.SkKagawad);
        }

        private BarangayOfficialViewModel ToViewModel(BarangayOfficial o) => new BarangayOfficialViewModel
        {
            Id = o.BarangayOfficialId,
            FullName = o.FullName,
            Position = o.Position,
            Committee = o.Committee,
            SignaturePath = o.SignaturePath,
            ProfileImagePath = o.ProfileImagePath,
            IsActive = o.IsActive
        };

        // Ibinabalik ang History view kasama ang na-type ng user (para hindi mawala).
        private IActionResult HistoryWithInput(BarangayOfficial official, OfficialHistoryViewModel model)
        {
            var vm = BuildHistoryViewModel(official);
            vm.NewPosition = model.NewPosition;
            vm.NewCommittee = model.NewCommittee;
            vm.NewStartYear = model.NewStartYear;
            vm.NewEndYear = model.NewEndYear;
            return View("History", vm);
        }

        // Sentralisadong validation para sa isang service-history term (Create at Edit).
        // `others` = umiiral na terms MALIBAN sa record na ineedit (o isasarang predecessor).
        // Nagbabalik ng error message (exact strings), o null kung wasto.
        private static string? ValidateServiceTerm(
            System.Collections.Generic.List<OfficialServiceHistory> others,
            string position, string committee, int startYear, int? endYear)
        {
            if (string.IsNullOrWhiteSpace(position))
                return "Position is required.";

            if (endYear.HasValue && endYear.Value < startYear)
                return "Start Year cannot be later than End Year.";

            // Isa lamang na kasalukuyang (Present) term bawat tao.
            if (!endYear.HasValue && others.Any(o => o.EndDate == null))
                return "This person already has a current service term.";

            // Exact duplicate: Position + Committee + Start Year + End Year.
            var pos = position.Trim();
            var com = (committee ?? string.Empty).Trim();
            if (others.Any(o =>
                    string.Equals((o.Position ?? string.Empty).Trim(), pos, System.StringComparison.OrdinalIgnoreCase) &&
                    string.Equals((o.Committee ?? string.Empty).Trim(), com, System.StringComparison.OrdinalIgnoreCase) &&
                    o.StartDate.Year == startYear &&
                    (o.EndDate?.Year) == endYear))
                return "This service history record already exists.";

            // Overlap sa umiiral na panahon (touching-at-boundary ay pinapayagan lamang
            // kapag consecutive-to-current — tingnan ang PeriodsConflict).
            if (others.Any(o => PeriodsConflict(startYear, endYear, o.StartDate.Year, o.EndDate?.Year)))
                return "This service period overlaps an existing term for this person.";

            return null;
        }

        // true kung tunay na nagkakapatong ang dalawang panahon (year-based).
        // null end = kasalukuyan (walang katapusan). Ang PAGDIKIT sa iisang boundary
        // year (ang mas huling term ay nagsisimula sa taon ng pagtatapos ng nauna) ay
        // itinuturing na transisyon — magkasunod na termino — kaya PINAPAYAGAN, maging
        // ang huling term ay Present (2023–2026 + 2026–Present) o sarado na
        // (2023–2026 + 2026–2030). Tunay na overlap lamang — kung saan ang mas huli ay
        // nagsisimula BAGO matapos ang nauna — ang hinaharangan.
        private static bool PeriodsConflict(int aStart, int? aEnd, int bStart, int? bEnd)
        {
            int aE = aEnd ?? int.MaxValue;
            int bE = bEnd ?? int.MaxValue;

            // Walang anumang pagsasalubong.
            if (aStart > bE || bStart > aE) return false;

            // Nagsasalubong sila. Alamin kung pagdikit lamang sa iisang boundary
            // (transition year) — pinapayagan para sa magkasunod na termino.
            int laterStart; int earlierEnd;
            if (aStart >= bStart) { laterStart = aStart; earlierEnd = bE; }
            else { laterStart = bStart; earlierEnd = aE; }

            if (laterStart == earlierEnd)
                return false; // magkasunod na termino — hati sa transition year

            return true; // tunay na overlap
        }

        private OfficialHistoryViewModel BuildHistoryViewModel(BarangayOfficial official) => new OfficialHistoryViewModel
        {
            OfficialId = official.BarangayOfficialId,
            OfficialName = official.FullName,
            CurrentPosition = official.Position,
            Terms = official.ServiceHistories
                .OrderByDescending(h => h.StartDate)
                .Select(h => new ServiceHistoryItem
                {
                    Id = h.Id,
                    Position = h.Position,
                    Committee = h.Committee,
                    StartDate = h.StartDate,
                    EndDate = h.EndDate,
                    Status = h.Status
                }).ToList()
        };

        // Ise-save ang isang naka-upload na file gamit ang natatanging pangalan; nagbabalik
        // ng filename (o "" kung walang file). Ginagamit para sa signatures at officials.
        private async Task<string> SaveUploadAsync(Microsoft.AspNetCore.Http.IFormFile? file, string subfolder)
        {
            if (file == null || file.Length == 0) return string.Empty;

            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", subfolder);
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            // Natatanging pangalan na hindi umaasa sa buong user-supplied filename (iwas traversal).
            var ext = Path.GetExtension(file.FileName);
            string uniqueFileName = Guid.NewGuid().ToString("N") + ext;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
            return uniqueFileName;
        }

        private void DeleteUpload(string? fileName, string subfolder)
        {
            if (string.IsNullOrEmpty(fileName)) return;
            try
            {
                // Huling segment lamang — iwas path traversal.
                var safe = Path.GetFileName(fileName);
                string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", subfolder, safe);
                if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
            }
            catch { /* best-effort */ }
        }
    }
}
