using System;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace BarangayCMS.Web.Controllers
{
    /// <summary>
    /// One centralized endpoint for switching the UI language. It saves the choice
    /// in the culture cookie (so it persists across refreshes, navigation, and every
    /// role/area) and returns the user to the SAME page they were on — it never
    /// redirects to the Dashboard or logs the user out.
    /// </summary>
    public class CultureController : Controller
    {
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Set(string culture, string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(culture))
            {
                Response.Cookies.Append(
                    CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                    new CookieOptions
                    {
                        Expires = DateTimeOffset.UtcNow.AddYears(1),
                        IsEssential = true,
                        HttpOnly = false,
                        SameSite = SameSiteMode.Lax
                    });
            }

            // Stay on the current page. LocalRedirect blocks open-redirects.
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            return Redirect("/");
        }
    }
}
