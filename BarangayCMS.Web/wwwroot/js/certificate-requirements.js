/*
 * Dynamic Certificate Requirements
 * --------------------------------
 * Watches a certificate-type <select> and, whenever the selection changes,
 * fetches that certificate's requirements from the database
 * (/CertificateRequest/RequirementsFor) and renders them beneath the dropdown.
 *
 * Requirements are DB-driven — nothing here is hardcoded. The container only
 * declares where to fetch from and which <select> to watch:
 *
 *   <div class="cert-requirements"
 *        id="certRequirements"
 *        data-endpoint="/CertificateRequest/RequirementsFor"
 *        data-select="#certificateSelect"
 *        data-by="name">   (name = send certificateType text; id = send certificateTypeId)
 *   </div>
 *
 * Required items render as checkboxes (name="confirmedRequirements") that must
 * be confirmed before the surrounding <form> can submit. Both client-side and
 * server-side validation identify exactly which required item is missing.
 */
(function () {
    "use strict";

    function el(tag, className, html) {
        var node = document.createElement(tag);
        if (className) node.className = className;
        if (html != null) node.innerHTML = html;
        return node;
    }

    function setState(body, cssState, iconClass, text) {
        body.innerHTML = "";
        var wrap = el("div", "cert-requirements__state" + (cssState ? " cert-requirements__state--" + cssState : ""));
        wrap.appendChild(el("i", iconClass));
        wrap.appendChild(el("span", null, text));
        body.appendChild(wrap);
    }

    function renderGroup(body, title, items) {
        if (!items.length) return;
        body.appendChild(el("div", "cert-requirements__group-title", title));
        var list = el("ul", "cert-requirements__list");

        items.forEach(function (r) {
            var li = el("li", "cert-req-item");
            li.dataset.reqId = r.id;
            li.dataset.required = r.required ? "true" : "false";

            var check = el("div", "cert-req-item__check");
            var input = document.createElement("input");
            input.type = "checkbox";
            input.className = "form-check-input cert-req-check";
            input.name = "confirmedRequirements";
            input.value = r.id;
            input.id = "req_" + r.id;
            check.appendChild(input);

            var text = el("div", "cert-req-item__text");
            var name = el("label", "cert-req-item__name");
            name.setAttribute("for", input.id);
            name.appendChild(document.createTextNode(r.name));
            name.appendChild(el("span",
                "cert-req-badge " + (r.required ? "cert-req-badge--required" : "cert-req-badge--optional"),
                r.required ? "Required" : "Optional"));
            text.appendChild(name);

            if (r.description) {
                text.appendChild(el("div", "cert-req-item__desc", r.description));
            }

            li.appendChild(check);
            li.appendChild(text);
            list.appendChild(li);
        });

        body.appendChild(list);
    }

    function render(body, items) {
        body.innerHTML = "";

        if (!items || !items.length) {
            setState(body, null, "bi bi-info-circle",
                "No additional requirements are currently listed.");
            return;
        }

        var required = items.filter(function (r) { return r.required; });
        var optional = items.filter(function (r) { return !r.required; });

        renderGroup(body, "Required Documents", required);
        renderGroup(body, "Optional Documents", optional);

        var note = el("p", "cert-requirements__note");
        note.appendChild(el("i", "bi bi-check2-circle"));
        note.appendChild(el("span", null,
            "Please prepare and confirm all required documents before submitting your request."));
        body.appendChild(note);
    }

    function load(container, body, select) {
        var value = select.value;
        if (!value) {
            setState(body, null, "bi bi-arrow-up-circle",
                "Select a certificate type to see its requirements.");
            return;
        }

        setState(body, null, "bi bi-hourglass-split", "Loading requirements...");

        var endpoint = container.dataset.endpoint || "/CertificateRequest/RequirementsFor";
        var by = (container.dataset.by || "name").toLowerCase();
        var url = endpoint + (endpoint.indexOf("?") >= 0 ? "&" : "?");
        url += by === "id"
            ? "certificateTypeId=" + encodeURIComponent(value)
            : "certificateType=" + encodeURIComponent(value);

        fetch(url, { headers: { "Accept": "application/json" } })
            .then(function (res) {
                if (!res.ok) throw new Error("HTTP " + res.status);
                return res.json();
            })
            .then(function (items) { render(body, items); })
            .catch(function () {
                setState(body, "error", "bi bi-exclamation-triangle",
                    "Unable to load certificate requirements. Please try again.");
            });
    }

    function attachValidation(container, form) {
        if (!form || form.dataset.certReqBound === "true") return;
        form.dataset.certReqBound = "true";

        form.addEventListener("submit", function (e) {
            var missing = [];
            container.querySelectorAll(".cert-req-item[data-required='true']").forEach(function (li) {
                var check = li.querySelector(".cert-req-check");
                if (check && !check.checked) {
                    li.classList.add("is-invalid");
                    var label = li.querySelector(".cert-req-item__name");
                    missing.push(label ? label.firstChild.textContent.trim() : "a required document");
                } else {
                    li.classList.remove("is-invalid");
                }
            });

            if (missing.length) {
                e.preventDefault();
                var body = container.querySelector(".cert-requirements__body");
                var old = container.querySelector(".cert-requirements__alert");
                if (old) old.remove();
                var alert = el("div", "alert alert-danger small mt-2 cert-requirements__alert");
                alert.innerHTML = "<strong>Please complete the following required document" +
                    (missing.length > 1 ? "s" : "") + ":</strong><ul class='mb-0 mt-1'>" +
                    missing.map(function (m) { return "<li>" + m + "</li>"; }).join("") + "</ul>";
                body.appendChild(alert);
                container.scrollIntoView({ behavior: "smooth", block: "center" });
            }
        });

        // Clear the invalid highlight as the user confirms each item.
        container.addEventListener("change", function (e) {
            if (e.target && e.target.classList.contains("cert-req-check")) {
                var li = e.target.closest(".cert-req-item");
                if (li && e.target.checked) li.classList.remove("is-invalid");
            }
        });
    }

    function init() {
        var containers = document.querySelectorAll(".cert-requirements");
        containers.forEach(function (container) {
            var selectSel = container.dataset.select;
            var select = selectSel ? document.querySelector(selectSel) : null;
            if (!select) return;

            var body = container.querySelector(".cert-requirements__body");
            if (!body) {
                body = el("div", "cert-requirements__body");
                container.appendChild(body);
            }

            select.addEventListener("change", function () { load(container, body, select); });

            // Preserve existing selection (edit / redisplay after validation error).
            load(container, body, select);

            attachValidation(container, container.closest("form"));
        });
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", init);
    } else {
        init();
    }
})();
