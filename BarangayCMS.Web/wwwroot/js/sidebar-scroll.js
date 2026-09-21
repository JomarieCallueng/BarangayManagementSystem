// Persists the admin/staff sidebar scroll position across full-page navigations.
// The sidebar (.app-sidebar) re-renders on every MVC page load, resetting its
// scroll to the top. We remember the last scrollTop per tab (sessionStorage) and
// restore it; on first load (no saved value) we bring the active item into view.
(function () {
    "use strict";

    var KEY = "app-sidebar-scroll";
    var sidebar = document.querySelector(".app-sidebar");
    if (!sidebar) return;

    function save() {
        try { sessionStorage.setItem(KEY, String(sidebar.scrollTop)); } catch (e) { /* ignore */ }
    }

    function restore() {
        var saved = null;
        try { saved = sessionStorage.getItem(KEY); } catch (e) { /* ignore */ }

        var value = saved === null ? NaN : parseInt(saved, 10);
        if (!isNaN(value) && value > 0) {
            sidebar.scrollTop = value;
        }

        // Guarantee the active item is visible so the user never has to scroll
        // down again (covers first load and direct-URL navigation too).
        var active = sidebar.querySelector(".app-nav__link.active");
        if (active && typeof active.scrollIntoView === "function") {
            var link = active.getBoundingClientRect();
            var box = sidebar.getBoundingClientRect();
            if (link.top < box.top || link.bottom > box.bottom) {
                active.scrollIntoView({ block: "center" });
            }
        }
    }

    // Throttled save while scrolling.
    var pending;
    sidebar.addEventListener("scroll", function () {
        if (pending) return;
        pending = window.setTimeout(function () { pending = null; save(); }, 100);
    }, { passive: true });

    // Also capture the exact position right before leaving the page.
    window.addEventListener("beforeunload", save);
    window.addEventListener("pagehide", save);

    restore();
})();
