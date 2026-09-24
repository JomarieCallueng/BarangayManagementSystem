/*
 * Client-side adapters for the app's custom validation attributes, wired into
 * the EXISTING jQuery-unobtrusive pipeline (loaded after jquery.validate).
 *   - personname : letters, spaces, hyphens, apostrophes, periods (>=1 letter)
 *   - phmobile   : exactly 11 digits starting 09 (09XXXXXXXXX)
 * Messages come from the server via data-val-* attributes, so they are already
 * localized (English / Filipino). Empty is allowed unless [Required] is present.
 *
 * Also adds smart phone-input behavior: digits only, max 11, numeric keyboard.
 */
(function () {
    "use strict";

    function ready(fn) {
        if (document.readyState !== "loading") fn();
        else document.addEventListener("DOMContentLoaded", fn);
    }

    // ---- jQuery-validate custom methods + unobtrusive adapters ----
    if (window.jQuery && window.jQuery.validator) {
        var $ = window.jQuery;

        $.validator.addMethod("personname", function (value, element) {
            if (this.optional(element)) return true;
            var v = (value || "").trim();
            try {
                return /^[\p{L} .'\-]+$/u.test(v) && /\p{L}/u.test(v);
            } catch (e) {
                // Older engines without Unicode property escapes.
                return /^[A-Za-zÀ-ÿ .'\-]+$/.test(v) && /[A-Za-zÀ-ÿ]/.test(v);
            }
        });

        $.validator.addMethod("phmobile", function (value, element) {
            if (this.optional(element)) return true;
            return /^09\d{9}$/.test((value || "").trim());
        });

        if ($.validator.unobtrusive && $.validator.unobtrusive.adapters) {
            $.validator.unobtrusive.adapters.addBool("personname");
            $.validator.unobtrusive.adapters.addBool("phmobile");
        }
    }

    // ---- Smart phone input: digits only, max 11, numeric keyboard ----
    ready(function () {
        var phoneInputs = document.querySelectorAll("input[data-val-phmobile]");
        Array.prototype.forEach.call(phoneInputs, function (input) {
            input.setAttribute("maxlength", "11");
            input.setAttribute("inputmode", "numeric");
            input.setAttribute("autocomplete", "tel");
            if (!input.getAttribute("placeholder")) {
                input.setAttribute("placeholder", "09XXXXXXXXX");
            }
            input.addEventListener("input", function () {
                var digits = (input.value || "").replace(/\D+/g, "").slice(0, 11);
                if (digits !== input.value) input.value = digits;
            });
        });
    });
})();
