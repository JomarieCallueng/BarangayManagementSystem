/*
 * Theme + layout presets for the Barangay Management System.
 *
 * Two responsibilities:
 *   1. applyStoredPreset() runs inline in <head> on EVERY page, before first
 *      paint, so the saved palette is in place with no flash of the default.
 *   2. The switcher UI (_ThemeSwitcher.cshtml) is only rendered on the Home
 *      page and wires itself up on DOMContentLoaded.
 *
 * The chosen preset is stored in localStorage and applies site-wide.
 */
(function (window, document) {
    "use strict";

    var STORAGE_KEY = "bms.preset";
    var DEFAULT_PRESET = "civic";

    // preset id -> { theme (palette), layout (home page structure) }
    var PRESETS = {
        civic: { theme: "civic", layout: "classic" },
        coastal: { theme: "coastal", layout: "split" },
        midnight: { theme: "midnight", layout: "compact" }
    };

    function apply(presetId) {
        var preset = PRESETS[presetId] || PRESETS[DEFAULT_PRESET];
        var root = document.documentElement;
        root.setAttribute("data-theme", preset.theme);
        root.setAttribute("data-layout", preset.layout);
        return preset;
    }

    function stored() {
        try {
            return window.localStorage.getItem(STORAGE_KEY) || DEFAULT_PRESET;
        } catch (e) {
            // Private mode / storage disabled — fall back to the default.
            return DEFAULT_PRESET;
        }
    }

    function save(presetId) {
        try {
            window.localStorage.setItem(STORAGE_KEY, presetId);
        } catch (e) {
            /* non-fatal: the preset still applies for this page view */
        }
    }

    var BmsTheme = {
        applyStoredPreset: function () {
            return apply(stored());
        },

        select: function (presetId) {
            if (!PRESETS[presetId]) {
                return;
            }
            apply(presetId);
            save(presetId);
        },

        current: stored,

        /* Called by the switcher partial once its markup is in the DOM. */
        initSwitcher: function () {
            var fab = document.querySelector("[data-theme-fab]");
            var panel = document.querySelector("[data-theme-panel]");
            if (!fab || !panel) {
                return;
            }

            var options = panel.querySelectorAll("[data-preset]");

            function markActive(presetId) {
                Array.prototype.forEach.call(options, function (option) {
                    var isActive = option.getAttribute("data-preset") === presetId;
                    option.setAttribute("aria-checked", isActive ? "true" : "false");
                });
            }

            function setPanel(open) {
                panel.setAttribute("data-open", open ? "true" : "false");
                fab.setAttribute("aria-expanded", open ? "true" : "false");
            }

            markActive(stored());

            fab.addEventListener("click", function (event) {
                event.stopPropagation();
                setPanel(panel.getAttribute("data-open") !== "true");
            });

            Array.prototype.forEach.call(options, function (option) {
                option.addEventListener("click", function () {
                    var presetId = option.getAttribute("data-preset");
                    BmsTheme.select(presetId);
                    markActive(presetId);
                });
            });

            // Click-away and Escape both dismiss the panel.
            document.addEventListener("click", function (event) {
                if (!panel.contains(event.target)) {
                    setPanel(false);
                }
            });

            document.addEventListener("keydown", function (event) {
                if (event.key === "Escape") {
                    setPanel(false);
                    fab.focus();
                }
            });
        }
    };

    window.BmsTheme = BmsTheme;

    // Apply immediately — this file is loaded in <head> before the body renders.
    BmsTheme.applyStoredPreset();
})(window, document);
