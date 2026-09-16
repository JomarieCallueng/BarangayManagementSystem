using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BarangayCMS.BLL.Interfaces;
using BarangayCMS.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarangayCMS.Web.Areas.Admin.Controllers
{
    /// <summary>
    /// Admin-only inbox para sa mga mensaheng ipinadala mula sa /Home/Contact
    /// "Send a Message" form. Walang resident account — Contact Number at Message lang.
    /// </summary>
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class MessagesController : Controller
    {
        private readonly IContactMessageService _messages;

        public MessagesController(IContactMessageService messages)
        {
            _messages = messages;
        }

        // GET: /Admin/Messages
        public async Task<IActionResult> Index(string? status, string? q, DateTime? from, DateTime? to)
        {
            var all = (await _messages.GetAllAsync()).ToList();

            IEnumerable<ContactMessageDTO> filtered = all;

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
                filtered = filtered.Where(m => m.Status == status);

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim().ToLowerInvariant();
                filtered = filtered.Where(m =>
                    m.ContactNumber.ToLowerInvariant().Contains(term) ||
                    m.Message.ToLowerInvariant().Contains(term));
            }

            if (from.HasValue)
                filtered = filtered.Where(m => m.CreatedAt.Date >= from.Value.Date);
            if (to.HasValue)
                filtered = filtered.Where(m => m.CreatedAt.Date <= to.Value.Date);

            var result = filtered.ToList();

            ViewBag.Status = status ?? "All";
            ViewBag.Query = q;
            ViewBag.From = from?.ToString("yyyy-MM-dd");
            ViewBag.To = to?.ToString("yyyy-MM-dd");

            ViewBag.TotalCount = all.Count;
            ViewBag.UnreadCount = all.Count(m => m.Status == "Unread");
            ViewBag.ReadCount = all.Count(m => m.Status == "Read");
            ViewBag.RepliedCount = all.Count(m => m.Status == "Replied");
            ViewBag.ArchivedCount = all.Count(m => m.Status == "Archived");

            return View(result);
        }

        // GET: /Admin/Messages/Details/5  (partial para sa modal)
        public async Task<IActionResult> Details(int id)
        {
            var message = await _messages.GetByIdAsync(id);
            if (message == null) return NotFound();
            return PartialView("_Details", message);
        }

        // POST: /Admin/Messages/MarkRead/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkRead(int id)
        {
            var ok = await _messages.MarkAsReadAsync(id);
            if (!ok) return NotFound();
            TempData["SuccessMessage"] = "Namarkahang nabasa na ang mensahe.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Messages/Reply
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reply(int id, string reply)
        {
            if (string.IsNullOrWhiteSpace(reply))
            {
                TempData["ErrorMessage"] = "Walang laman ang sagot.";
                return RedirectToAction(nameof(Index));
            }

            var ok = await _messages.ReplyAsync(id, reply);
            if (!ok) return NotFound();

            TempData["SuccessMessage"] = "✅ Naitala ang iyong sagot sa mensahe.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Messages/Archive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Archive(int id)
        {
            var ok = await _messages.ArchiveAsync(id);
            if (!ok) return NotFound();
            TempData["SuccessMessage"] = "Na-archive ang mensahe.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Messages/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _messages.DeleteAsync(id);
            if (!ok) return NotFound();
            TempData["SuccessMessage"] = "Permanente nang natanggal ang mensahe.";
            return RedirectToAction(nameof(Index));
        }
    }
}
