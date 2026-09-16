/*
 * form-persist.js
 * -------------------------------------------------------------
 * Universal form draft persistence para sa buong Barangay CMS.
 *
 * Layunin: Kung ang user ay umalis, bumalik, o aksidenteng mag-navigate
 * palayo habang pinupunan ang isang form, ang mga naitype na datos ay
 * mananatili pagbalik niya — hindi na kailangang i-type muli.
 * Ang draft ay bubura LAMANG kapag matagumpay na na-submit ang form.
 *
 * Paggana: awtomatikong sini-save ang bawat field papunta sa localStorage
 * habang nagta-type ang user, at ibinabalik ito sa susunod na pag-load ng
 * parehong page. Client-side lang ito kaya gumagana kahit "Back" button,
 * accidental close, o pag-navigate palayo ang ginamit.
 *
 * Naaangkop sa lahat ng role: Resident, Staff, at Admin (naka-load sa
 * lahat ng _Layout.cshtml).
 */
(function () {
    "use strict";

    // Ilang araw bago mag-expire ang isang naka-save na draft (para hindi
    // mabuhay magpakailanman ang napakalumang datos).
    var MAX_AGE_MS = 3 * 24 * 60 * 60 * 1000; // 3 araw
    var PREFIX = "bms_draft::";

    // Huwag i-save ang mga sensitibo o hindi dapat i-restore na field.
    function isPersistable(el) {
        if (!el || !el.name) return false;
        if (el.disabled) return false;
        var type = (el.type || "").toLowerCase();
        if (type === "password" || type === "file" || type === "hidden") return false;
        if (type === "submit" || type === "button" || type === "reset" || type === "image") return false;
        // Anti-forgery token at anumang minarkahang huwag i-persist.
        if (el.name === "__RequestVerificationToken") return false;
        if (el.hasAttribute("data-no-persist")) return false;
        if (el.closest && el.closest("[data-no-persist]")) return false;
        return true;
    }

    // Bumuo ng natatanging susi para sa isang form base sa page path at
    // sa pagkakakilanlan ng form (id, action, o index).
    function formKey(form, index) {
        var path = window.location.pathname.replace(/\/+$/, "") || "/";
        var ident = form.getAttribute("id")
            || form.getAttribute("name")
            || form.getAttribute("action")
            || ("form" + index);
        return PREFIX + path + "::" + ident;
    }

    function collect(form) {
        var data = {};
        var els = form.elements;
        for (var i = 0; i < els.length; i++) {
            var el = els[i];
            if (!isPersistable(el)) continue;
            var type = (el.type || "").toLowerCase();
            if (type === "checkbox") {
                data["cb:" + el.name + ":" + (el.value || "on")] = el.checked;
            } else if (type === "radio") {
                if (el.checked) data["rd:" + el.name] = el.value;
            } else {
                data["v:" + el.name + ":" + fieldIndex(form, el)] = el.value;
            }
        }
        return data;
    }

    // Para sa mga field na may parehong pangalan (hal. array inputs),
    // gamitin ang posisyon para tumpak ang pag-restore.
    function fieldIndex(form, target) {
        var same = form.querySelectorAll('[name="' + cssEscape(target.name) + '"]');
        for (var i = 0; i < same.length; i++) {
            if (same[i] === target) return i;
        }
        return 0;
    }

    function cssEscape(value) {
        if (window.CSS && window.CSS.escape) return window.CSS.escape(value);
        return String(value).replace(/["\\\]\[]/g, "\\$&");
    }

    function save(form, index) {
        try {
            var payload = { t: Date.now(), d: collect(form) };
            window.localStorage.setItem(formKey(form, index), JSON.stringify(payload));
        } catch (e) { /* storage full/blocked — huwag i-crash ang page */ }
    }

    function clear(form, index) {
        try { window.localStorage.removeItem(formKey(form, index)); } catch (e) { }
    }

    function restore(form, index) {
        var raw;
        try { raw = window.localStorage.getItem(formKey(form, index)); } catch (e) { return; }
        if (!raw) return;

        var payload;
        try { payload = JSON.parse(raw); } catch (e) { clear(form, index); return; }

        // Burahin ang expired na draft.
        if (!payload || !payload.d || (Date.now() - (payload.t || 0)) > MAX_AGE_MS) {
            clear(form, index);
            return;
        }

        var data = payload.d;
        var restoredAny = false;
        var els = form.elements;
        for (var i = 0; i < els.length; i++) {
            var el = els[i];
            if (!isPersistable(el)) continue;
            var type = (el.type || "").toLowerCase();
            if (type === "checkbox") {
                var ck = data["cb:" + el.name + ":" + (el.value || "on")];
                if (typeof ck === "boolean" && el.checked !== ck) { el.checked = ck; restoredAny = true; }
            } else if (type === "radio") {
                var rv = data["rd:" + el.name];
                if (rv !== undefined && el.value === rv && !el.checked) { el.checked = true; restoredAny = true; }
            } else {
                var key = "v:" + el.name + ":" + fieldIndex(form, el);
                if (data[key] !== undefined && data[key] !== "" && el.value !== data[key]) {
                    el.value = data[key];
                    restoredAny = true;
                }
            }
        }

        if (restoredAny) {
            showNotice(form);
            // Ipaalam sa ibang script (hal. select2/choices) na nagbago ang value.
            try { form.dispatchEvent(new Event("bms:restored", { bubbles: true })); } catch (e) { }
        }
    }

    // Maliit, hindi nakakaabalang paalala na naibalik ang draft.
    function showNotice(form) {
        if (form.querySelector(".bms-draft-notice")) return;
        var note = document.createElement("div");
        note.className = "bms-draft-notice";
        note.setAttribute("role", "status");
        note.style.cssText = "margin:0 0 12px;padding:8px 12px;border-radius:8px;" +
            "background:#e7f1ff;color:#0b5ed7;border:1px solid #b6d4fe;font-size:.85rem;" +
            "display:flex;align-items:center;gap:8px;";
        note.innerHTML = '<span aria-hidden="true">&#128221;</span>' +
            '<span>Naibalik ang hindi pa naisusumite mong entry. ' +
            '<a href="#" class="bms-draft-discard" style="color:#0b5ed7;font-weight:600;">I-clear ito</a>.</span>';
        form.insertBefore(note, form.firstChild);
    }

    function init() {
        var forms = document.querySelectorAll("form");
        forms.forEach(function (form, index) {
            if (form.getAttribute("data-no-persist") !== null) return; // opt-out per form
            // Huwag i-persist ang mga search/filter (GET) form — ayaw natin
            // maiwang naka-fill ang search box.
            var method = (form.getAttribute("method") || "get").toLowerCase();
            if (method !== "post") return;

            restore(form, index);

            // I-save habang nagta-type / nagbabago.
            var handler = function () { save(form, index); };
            form.addEventListener("input", handler);
            form.addEventListener("change", handler);

            // Linisin ang draft kapag matagumpay na na-submit ang form.
            // Ang 'submit' event ay hindi tumatakbo kapag na-block ng
            // client-side validation, kaya ligtas ito.
            form.addEventListener("submit", function () { clear(form, index); });

            // Manual na pag-clear mula sa paalala.
            form.addEventListener("click", function (ev) {
                var t = ev.target;
                if (t && t.classList && t.classList.contains("bms-draft-discard")) {
                    ev.preventDefault();
                    clear(form, index);
                    form.reset();
                    var n = form.querySelector(".bms-draft-notice");
                    if (n) n.remove();
                }
            });
        });
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", init);
    } else {
        init();
    }
})();
