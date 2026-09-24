using System;
using System.Collections.Generic;
using BarangayCMS.Web.Areas.Admin.Models;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BarangayCMS.Web.Services
{
    // ============================================================
    // EXECUTIVE SUMMARY REPORT — server-generated official PDF.
    // Rendered with QuestPDF (managed) and streamed straight to the
    // browser as a file download, so clicking "Export PDF Summary"
    // downloads directly with no on-screen preview and no page cut-off.
    //
    // Layout is intentionally COMPACT so the full summary fits on a
    // SINGLE A4 sheet. Sections that must never split across pages are
    // wrapped in .ShowEntire() so, if a barangay's data ever grows past
    // one page, the break happens between whole blocks — never mid-row.
    // Follows the standard format of a Philippine barangay official
    // document: national letterhead with barangay + city seals,
    // document-control strip, and signatory certification.
    // ============================================================
    public sealed class ExecutiveSummaryDocument : IDocument
    {
        // ---- Formal government-document palette ----
        private const string Navy = "#0f2c52";   // official dark navy
        private const string Accent = "#1d4ed8";  // brand blue accent
        private const string Ink = "#1f2937";
        private const string Muted = "#6b7280";
        private const string Line = "#d6dce5";
        private const string SoftBg = "#f4f7fb";
        private const string Gold = "#b8860b";   // subtle seal/accent tone

        private readonly ReportsDashboardViewModel _m;
        private readonly string _barangayName;
        private readonly string _cityMunicipality;
        private readonly string _region;
        private readonly string _district;
        private readonly DateTime _generatedAt;
        private readonly string _referenceNo;
        private readonly byte[]? _leftSeal;   // Barangay seal (circular)
        private readonly byte[]? _rightSeal;  // City seal (triangle)

        public ExecutiveSummaryDocument(
            ReportsDashboardViewModel model,
            string barangayName,
            string cityMunicipality,
            DateTime generatedAt,
            string region = "Metropolitan Manila",
            string district = "District IV",
            byte[]? leftSeal = null,
            byte[]? rightSeal = null)
        {
            _m = model;
            _barangayName = string.IsNullOrWhiteSpace(barangayName) ? "________________" : barangayName.Trim();
            _cityMunicipality = string.IsNullOrWhiteSpace(cityMunicipality) ? "________________" : cityMunicipality.Trim();
            _region = region?.Trim() ?? string.Empty;
            _district = district?.Trim() ?? string.Empty;
            _generatedAt = generatedAt;
            _referenceNo = $"BMS-ESR-{_generatedAt:yyyyMMdd}-{_generatedAt:HHmm}";
            _leftSeal = leftSeal;
            _rightSeal = rightSeal;
        }

        // "BARANGAY TATALON" without doubling the word if the stored name
        // already begins with "Barangay".
        private string BarangayHeadline()
        {
            var upper = _barangayName.ToUpperInvariant();
            return upper.StartsWith("BARANGAY") ? upper : "BARANGAY " + upper;
        }

        // Metric groups shown in the body (official domain grouping).
        private IEnumerable<(string Group, (string Label, string Value)[] Rows)> Groups()
        {
            yield return ("A. Population & Civil Registry", new[]
            {
                ("Total Registered Residents", $"{_m.TotalResidents:N0}"),
                ("Total Health Center Records", $"{_m.TotalHealthRecords:N0}"),
            });
            yield return ("B. Peace, Order & Public Safety", new[]
            {
                ("Total Blotter & Incident Cases Logged", $"{_m.TotalComplaints:N0}"),
                ("Active Evacuees Logged (Disaster Response)", $"{_m.ActiveEvacuees:N0}"),
            });
            yield return ("C. Public Services Delivered", new[]
            {
                ("Total Official Certificates Issued", $"{_m.TotalCertificatesIssued:N0}"),
                ("Active / Ongoing Infrastructure Projects", $"{_m.ActiveProjects:N0}"),
            });
            yield return ("D. Fiscal Management", new[]
            {
                ("Total Approved Budget Allocation", $"Php {_m.TotalBudget:N2}"),
            });
        }

        public DocumentMetadata GetMetadata() => new DocumentMetadata
        {
            Title = $"{BarangayHeadline()} — Executive Summary Report",
            Author = $"Office of the Sangguniang Barangay, {_barangayName}",
            Subject = "Executive Analytics & Summary Report",
        };

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(32);
                page.DefaultTextStyle(t => t.FontSize(9.5f).FontColor(Ink).LineHeight(1.25f));

                page.Header().Element(ComposeHeader);
                page.Content().PaddingVertical(6).Element(ComposeBody);
                page.Footer().Element(ComposeFooter);
            });
        }

        // -------------------- HEADER (national letterhead) --------------------
        private void ComposeHeader(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().Row(row =>
                {
                    // Left: Barangay seal (circular)
                    row.ConstantItem(54).AlignMiddle().AlignCenter().Element(c => Seal(c, _leftSeal, "BARANGAY", "SEAL"));

                    // Center: official letterhead text
                    row.RelativeItem().PaddingHorizontal(8).AlignCenter().Column(center =>
                    {
                        center.Item().AlignCenter().Text("Republic of the Philippines").FontSize(9.5f).FontColor(Ink);
                        if (!string.IsNullOrWhiteSpace(_region))
                            center.Item().AlignCenter().Text(_region).FontSize(9.5f).FontColor(Ink);
                        center.Item().AlignCenter().Text(CityDistrictLine()).FontSize(9.5f).FontColor(Ink);
                        center.Item().AlignCenter().Text(BarangayHeadline()).FontSize(12.5f).Bold().FontColor(Navy);
                        center.Item().AlignCenter().Text("OFFICE OF THE SANGGUNIANG BARANGAY").FontSize(8).FontColor(Muted).Bold();
                    });

                    // Right: City seal (triangle)
                    row.ConstantItem(70).AlignMiddle().AlignCenter().Element(c => Seal(c, _rightSeal, "OFFICIAL", "SEAL"));
                });

                // Double rule — classic official letterhead separator.
                col.Item().PaddingTop(6).BorderBottom(2).BorderColor(Navy);
                col.Item().PaddingTop(1.5f).BorderBottom(0.75f).BorderColor(Gold);
            });
        }

        private string CityDistrictLine()
        {
            if (!string.IsNullOrWhiteSpace(_district))
                return $"{_cityMunicipality}  •  {_district}";
            return _cityMunicipality;
        }

        private void Seal(IContainer container, byte[]? bytes, string top, string bottom)
        {
            if (bytes != null && bytes.Length > 0)
            {
                // Scales to the column width and preserves aspect ratio.
                container.Image(bytes);
                return;
            }

            // Fallback placeholder if a seal image is unavailable.
            container.Height(50).Width(50)
                .Border(1.25f).BorderColor(Gold).Background(SoftBg)
                .AlignMiddle().AlignCenter()
                .Column(col =>
                {
                    col.Item().AlignCenter().Text(top).FontSize(6.5f).Bold().FontColor(Gold);
                    col.Item().AlignCenter().Text("★").FontSize(10).FontColor(Gold);
                    col.Item().AlignCenter().Text(bottom).FontSize(6.5f).Bold().FontColor(Gold);
                });
        }

        // -------------------- BODY --------------------
        private void ComposeBody(IContainer container)
        {
            container.Column(col =>
            {
                col.Spacing(5);

                // Title block
                col.Item().AlignCenter().Text("EXECUTIVE SUMMARY REPORT")
                    .FontSize(14).Bold().FontColor(Navy).LetterSpacing(0.03f);
                col.Item().AlignCenter().Text($"Barangay Operational Analytics  •  For the period ending {_generatedAt:MMMM dd, yyyy} (FY {_generatedAt:yyyy})")
                    .FontSize(8.5f).FontColor(Muted).Italic();

                // Document control strip
                col.Item().Element(ComposeControlStrip);

                // Preamble
                col.Item().Text(text =>
                {
                    text.DefaultTextStyle(t => t.FontSize(9).FontColor(Ink).LineHeight(1.3f));
                    text.Span("Pursuant to Republic Act No. 7160 (Local Government Code of 1991), this Executive Summary "
                            + "consolidates the key operational metrics of ");
                    text.Span($"{BarangayHeadline()}, {_cityMunicipality}").Bold();
                    text.Span(", as officially recorded in the Barangay Management Information System (BMIS) as of ");
                    text.Span(_generatedAt.ToString("MMMM dd, yyyy")).Bold();
                    text.Span(", for the reference of the Punong Barangay and the Sangguniang Barangay.");
                });

                // Section I — metrics (kept together on one page)
                col.Item().Column(sec =>
                {
                    sec.Spacing(4);
                    sec.Item().Element(c => SectionTitle(c, "I.", "System Metrics Overview"));
                    sec.Item().ShowEntire().Element(ComposeMetricsTable);
                });

                // Section II — observations (kept together)
                col.Item().ShowEntire().Column(sec =>
                {
                    sec.Spacing(4);
                    sec.Item().Element(c => SectionTitle(c, "II.", "Summary of Observations"));
                    sec.Item().Element(ComposeObservations);
                });

                // Section III — certification / signatories (kept together)
                col.Item().ShowEntire().Column(sec =>
                {
                    sec.Spacing(4);
                    sec.Item().Element(c => SectionTitle(c, "III.", "Certification & Attestation"));
                    sec.Item().Text(
                        "I hereby certify that the figures reflected herein are system-generated from the official "
                        + "records maintained in the BMIS and are true and accurate as of the time of generation.")
                        .FontSize(9).FontColor(Ink).Italic();
                    sec.Item().PaddingTop(6).Element(ComposeSignatories);
                });
            });
        }

        private void ComposeControlStrip(IContainer container)
        {
            container.Border(0.75f).BorderColor(Line).Background(SoftBg).Table(table =>
            {
                table.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); });

                void Cell(string label, string value)
                {
                    table.Cell().Padding(6).Column(col =>
                    {
                        col.Item().Text(label).FontSize(7).Bold().FontColor(Muted).LetterSpacing(0.05f);
                        col.Item().Text(value).FontSize(9.5f).Bold().FontColor(Navy);
                    });
                }

                Cell("REFERENCE NO.", _referenceNo);
                Cell("DATE GENERATED", _generatedAt.ToString("MMM dd, yyyy • hh:mm tt"));
                Cell("CLASSIFICATION", "For Official Use Only");
            });
        }

        private void ComposeMetricsTable(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(30);   // No.
                    columns.RelativeColumn();      // Metric
                    columns.ConstantColumn(115);   // Value
                });

                // Header row
                table.Cell().Background(Navy).PaddingVertical(4).PaddingHorizontal(8).Text("#").FontColor("#ffffff").Bold().FontSize(8.5f);
                table.Cell().Background(Navy).PaddingVertical(5).PaddingHorizontal(8).Text("METRIC CATEGORY").FontColor("#ffffff").Bold().FontSize(8.5f);
                table.Cell().Background(Navy).PaddingVertical(5).PaddingHorizontal(8).AlignRight().Text("CURRENT VALUE").FontColor("#ffffff").Bold().FontSize(8.5f);

                int index = 0;
                foreach (var group in Groups())
                {
                    // Group subheader spanning all columns
                    table.Cell().ColumnSpan(3).Background("#eaf0f9").PaddingVertical(3.5f).PaddingHorizontal(8)
                        .Text(group.Group).Bold().FontSize(8.5f).FontColor(Accent);

                    foreach (var (label, value) in group.Rows)
                    {
                        index++;
                        var zebra = index % 2 == 0 ? SoftBg : "#ffffff";

                        table.Cell().Background(zebra).BorderBottom(0.5f).BorderColor(Line).PaddingVertical(4).PaddingHorizontal(8)
                            .Text($"{index}").FontSize(9).FontColor(Muted);
                        table.Cell().Background(zebra).BorderBottom(0.5f).BorderColor(Line).PaddingVertical(4).PaddingHorizontal(8)
                            .Text(label).FontSize(9.5f);
                        table.Cell().Background(zebra).BorderBottom(0.5f).BorderColor(Line).PaddingVertical(4).PaddingHorizontal(8)
                            .AlignRight().Text(value).Bold().FontSize(10).FontColor(Navy);
                    }
                }
            });
        }

        private void ComposeObservations(IContainer container)
        {
            var items = new List<string>
            {
                $"{_m.TotalResidents:N0} registered resident record(s) are maintained in the civil registry as of the reporting date.",
                $"{_m.TotalCertificatesIssued:N0} official certificate(s) have been issued to constituents in the delivery of frontline services.",
                $"{_m.ActiveProjects:N0} infrastructure project(s) are presently active or ongoing under barangay development programs.",
                $"{_m.TotalComplaints:N0} blotter/incident case(s) have been logged for peace, order, and public-safety monitoring.",
                $"Php {_m.TotalBudget:N2} approved budget allocation is on record for the barangay's fiscal management.",
            };

            container.Column(col =>
            {
                col.Spacing(2.5f);
                foreach (var it in items)
                {
                    col.Item().Row(row =>
                    {
                        row.ConstantItem(13).Text("•").FontColor(Accent).Bold();
                        row.RelativeItem().Text(it).FontSize(9);
                    });
                }
            });
        }

        private void ComposeSignatories(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Element(c => SignatureBlock(c, "Prepared by:", "Barangay Secretary"));
                row.ConstantItem(40);
                row.RelativeItem().Element(c => SignatureBlock(c, "Approved by:", "Punong Barangay"));
            });
        }

        private void SignatureBlock(IContainer container, string caption, string title)
        {
            container.Column(col =>
            {
                col.Item().Text(caption).FontSize(9).FontColor(Muted);
                col.Item().PaddingTop(14).BorderBottom(1).BorderColor(Ink);
                col.Item().PaddingTop(2).Text("(Signature over Printed Name)").FontSize(7.5f).FontColor(Muted).Italic();
                col.Item().Text(title).FontSize(9).Bold().FontColor(Navy);
            });
        }

        private void SectionTitle(IContainer container, string number, string title)
        {
            container.BorderBottom(1.5f).BorderColor(Accent).PaddingBottom(2).Row(row =>
            {
                row.ConstantItem(22).Text(number).Bold().FontColor(Accent).FontSize(11);
                row.RelativeItem().AlignMiddle().Text(title).Bold().FontColor(Navy).FontSize(11);
            });
        }

        // -------------------- FOOTER --------------------
        private void ComposeFooter(IContainer container)
        {
            container.BorderTop(1).BorderColor(Line).PaddingTop(5).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("This is a system-generated document. It is not valid without the official signature and dry seal of the barangay.")
                        .FontSize(7).FontColor(Muted).Italic();
                    col.Item().Text($"Ref: {_referenceNo}  •  Barangay Management Information System (BMIS)")
                        .FontSize(7).FontColor(Muted);
                });
                row.ConstantItem(90).AlignRight().AlignBottom().Text(text =>
                {
                    text.DefaultTextStyle(t => t.FontSize(7.5f).FontColor(Muted));
                    text.Span("Page ");
                    text.CurrentPageNumber();
                    text.Span(" of ");
                    text.TotalPages();
                });
            });
        }
    }
}
