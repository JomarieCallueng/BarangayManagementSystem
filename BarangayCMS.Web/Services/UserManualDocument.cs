using System.Collections.Generic;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BarangayCMS.Web.Services
{
    // ============================================================
    // USER MANUAL — single source of truth for the printable PDF.
    // Content describes the REAL modules/workflows that exist in the
    // system (Admin, Staff/Encoder, and Resident/Public usage). Both
    // /Admin/Help/Manual and /Staff/Help/Manual serve this same PDF so
    // there is no duplicate manual. Rendered with QuestPDF (managed).
    // ============================================================
    public sealed class UserManualDocument : IDocument
    {
        // Brand palette (matches the app's primary/brand tone).
        private const string Brand = "#0d6efd";
        private const string BrandDark = "#0a4fb4";
        private const string Ink = "#1f2937";
        private const string Muted = "#6b7280";
        private const string Line = "#e5e7eb";
        private const string SoftBg = "#f3f6fb";

        private static readonly (string Icon, string Name, string Desc)[] Modules =
        {
            ("Dashboard", "Dashboard", "Buod ng mga pangunahing bilang at aktibidad ng barangay: residents, certificate requests, complaints, at reports."),
            ("Residents", "Residents", "Pamamahala ng resident records — magdagdag, mag-edit, at maghanap ng residente kasama ang demographics."),
            ("Certificates", "Certificates", "Pagproseso ng certificate requests at pamamahala ng certificate types na may dynamic na requirements at fees."),
            ("Complaints", "Complaints / Blotter", "Pagtanggap, pag-log, at pag-asikaso ng mga reklamo ng residente."),
            ("Messages", "Messages", "Inbox ng mga mensaheng ipinadala sa public Contact form. May unread badge sa sidebar at header (walang resident login)."),
            ("Projects", "Projects", "Pagsubaybay sa mga proyekto ng barangay at ang kanilang status."),
            ("Budget", "Budget Tracker", "Pagtatala ng taunang badyet, alokasyon kada programa, income, at disbursement/gastos."),
            ("Disaster", "Disaster Risk", "Pamamahala ng disaster incidents, evacuation centers at status, at Emergency SMS alerts (Semaphore)."),
            ("Health", "Health Center", "Mga clinical/medical records at immunization logs ng barangay."),
            ("Environment", "Environment", "Mga aktibidad at programa ukol sa kalikasan, kalinisan, at waste collection."),
            ("Officials", "Officials", "Pamamahala ng Barangay at SK officials — profile, committee, at service history, na may structure validation (limits, unique names/committees)."),
            ("Announcements", "Announcements", "Pag-post at pamamahala ng mga anunsyo para makita ng publiko."),
            ("Reports", "Reports", "Pagbuo ng mga summary/ulat mula sa aktwal na datos ng barangay."),
            ("Users", "User Accounts", "Pamamahala ng mga user account at roles (Admin at Staff/Encoder)."),
            ("Settings", "Settings", "System configuration tulad ng barangay profile at certificate templates/requirements."),
        };

        private static readonly (string Role, string Access, string[] Duties)[] Roles =
        {
            ("Administrator", "Buong access sa lahat ng module ng Admin portal.",
                new[]{
                    "Pamahalaan ang residents, officials, budget, disaster, health, environment, at reports.",
                    "I-configure ang certificate types, requirements, at barangay profile sa Settings.",
                    "Mag-manage ng user accounts at magpadala ng Emergency SMS alerts.",
                }),
            ("Staff / Encoder", "Limitadong access — pang-encode/records na module lamang.",
                new[]{
                    "Mag-encode at mag-update ng residents, certificates, at complaints.",
                    "Mag-log ng disaster incidents, health records, at environment activities.",
                    "Mag-post ng announcements at tumingin ng reports.",
                }),
            ("Resident / Public", "Walang login — pampublikong website lamang.",
                new[]{
                    "Magpadala ng mensahe sa pamamagitan ng Contact form (dumadating sa Admin inbox).",
                    "Tingnan ang mga announcements, barangay officials/profile, at evacuation status.",
                    "Mag-request ng certificate at tingnan ang mga fees/requirements.",
                }),
        };

        private static readonly (string Title, string[] Steps)[] Workflows =
        {
            ("Magdagdag ng Resident", new[]{
                "Buksan ang Residents mula sa sidebar.",
                "I-click ang Add / Magdagdag.",
                "Punan ang mga detalye ng residente, tapos i-save.",
                "Lalabas na ang residente sa listahan at maaari nang i-edit o hanapin sa search.",
            }),
            ("Mag-proseso ng Certificate Request", new[]{
                "Buksan ang Certificates at piliin ang request na ipoproseso.",
                "I-review ang mga detalye at requirements, tapos i-update ang status (hal. Approved / Released).",
                "I-configure ang certificate types, requirements, at fees sa Settings -> Certificate Templates.",
            }),
            ("Pamahalaan ang Barangay/SK Officials at Service History", new[]{
                "Buksan ang Officials — hati sa Sangguniang Barangay at Sangguniang Kabataan.",
                "I-click ang Add Council Member; naka-disable ang mga posisyong puno na.",
                "Awtomatikong tinutukoy ang committee ayon sa posisyon (Captain/Secretary/Treasurer = walang komite).",
                "Sa Manage Service History, isa lamang ang Present/Current na termino; ang bagong termino ay awtomatikong nagsasara ng lumang termino (reelection).",
            }),
            ("Magtala sa Budget Tracker", new[]{
                "Buksan ang Budget Tracker at pumili ng fiscal year.",
                "I-set ang Annual Budget, tapos magdagdag ng budget category/program.",
                "Gamitin ang (+) para mag-record ng income at (–) para mag-record ng gastos/disbursement.",
            }),
            ("Magpadala ng Emergency SMS Alert", new[]{
                "Buksan ang Disaster Risk (Admin lamang).",
                "Piliin ang emergency type, severity, at recipients (lahat, kada purok, o piling residente).",
                "I-preview ang bilang ng recipients, tapos kumpirmahin para ipadala sa pamamagitan ng Semaphore.",
            }),
            ("Sagutin ang Public Messages", new[]{
                "I-click ang envelope icon sa header o ang Messages sa sidebar (may unread badge).",
                "Buksan ang mensahe para makita ang detalye ng nagpadala.",
                "Markahan bilang nabasa o sumagot ayon sa proseso ng barangay.",
            }),
            ("Mag-post ng Announcement", new[]{
                "Buksan ang Announcements at i-click ang Add.",
                "Isulat ang pamagat at nilalaman, tapos i-publish.",
                "Makikita agad ito ng publiko sa public Announcements page.",
            }),
        };

        private static readonly string[] Tips =
        {
            "Kapag hindi lumalabas ang mga pagbabago, i-refresh ang pahina.",
            "Sa Officials, ang mga posisyong puno na ay naka-disable — i-deactivate muna ang kasalukuyan bago magdagdag ng bago.",
            "Kung na-block ang isang aksyon, basahin ang pulang mensahe sa itaas ng form para sa dahilan.",
            "Ang Messages badge ay galing sa aktwal na unread na mensahe sa database.",
            "Gamitin ang search bar sa Help System para mabilis na mahanap ang isang paksa.",
        };

        public DocumentMetadata GetMetadata() => new DocumentMetadata
        {
            Title = "Barangay Management System — User Manual",
            Author = "Barangay Management Information System",
            Subject = "User Manual",
        };

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(t => t.FontSize(10).FontColor(Ink).LineHeight(1.35f));

                page.Header().Element(ComposeHeader);
                page.Content().PaddingVertical(14).Element(ComposeBody);
                page.Footer().Element(ComposeFooter);
            });
        }

        private void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Barangay Management System").FontSize(16).Bold().FontColor(BrandDark);
                    col.Item().Text("User Manual — Gabay sa Paggamit").FontSize(10).FontColor(Muted);
                });
                row.ConstantItem(120).AlignRight().AlignMiddle()
                   .Text("Admin • Staff • Public").FontSize(9).FontColor(Brand).Bold();
            });
        }

        private void ComposeFooter(IContainer container)
        {
            container.BorderTop(1).BorderColor(Line).PaddingTop(6).Row(row =>
            {
                row.RelativeItem().Text("© 2026 Barangay Management Information System")
                   .FontSize(8).FontColor(Muted);
                row.ConstantItem(120).AlignRight().Text(text =>
                {
                    text.DefaultTextStyle(t => t.FontSize(8).FontColor(Muted));
                    text.Span("Pahina ");
                    text.CurrentPageNumber();
                    text.Span(" / ");
                    text.TotalPages();
                });
            });
        }

        private void ComposeBody(IContainer container)
        {
            container.Column(col =>
            {
                col.Spacing(16);

                // Intro
                col.Item().Element(c => Section(c, "1", "Panimula (Introduction)"));
                col.Item().Text(
                    "Ang Barangay Management System ay isang web-based na sistema para sa pang-araw-araw " +
                    "na operasyon ng barangay: pamamahala ng residente, certificate, reklamo, badyet, " +
                    "disaster response, kalusugan, kalikasan, officials, announcements, at reports. " +
                    "Sinasaklaw ng manwal na ito ang tatlong uri ng gumagamit: Administrator, Staff/Encoder, " +
                    "at Resident/Public.");

                // Getting started / navigation
                col.Item().Element(c => Section(c, "2", "Pag-sign In at Navigation"));
                col.Item().Element(c => Bullets(c, new[]
                {
                    "Mag-sign in gamit ang iyong account. Ang portal na makikita mo ay depende sa iyong role.",
                    "Nasa kaliwang bahagi ang sidebar navigation — dito matatagpuan ang lahat ng module.",
                    "Nasa itaas (topbar) ang mga notification tulad ng unread Messages badge.",
                    "Ang Help System at User Manual ay nasa ilalim ng sidebar, sa ilalim ng divider.",
                }));

                // Roles
                col.Item().Element(c => Section(c, "3", "Mga Uri ng Gumagamit (User Roles)"));
                foreach (var r in Roles)
                {
                    col.Item().Element(c => RoleCard(c, r.Role, r.Access, r.Duties));
                }

                // Modules
                col.Item().Element(c => Section(c, "4", "Pangunahing Modules"));
                col.Item().Element(ComposeModuleTable);

                // Workflows
                col.Item().Element(c => Section(c, "5", "Mahahalagang Workflow (Step-by-step)"));
                foreach (var w in Workflows)
                {
                    col.Item().Element(c => WorkflowCard(c, w.Title, w.Steps));
                }

                // Tips
                col.Item().Element(c => Section(c, "6", "Mga Tip at Troubleshooting"));
                col.Item().Element(c => Bullets(c, Tips));
            });
        }

        private void ComposeModuleTable(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(130);
                    columns.RelativeColumn();
                });

                foreach (var m in Modules)
                {
                    table.Cell().Border(0.5f).BorderColor(Line).Background(SoftBg).Padding(6)
                        .Text(m.Name).Bold().FontColor(BrandDark).FontSize(9.5f);
                    table.Cell().Border(0.5f).BorderColor(Line).Padding(6)
                        .Text(m.Desc).FontSize(9.5f);
                }
            });
        }

        private void RoleCard(IContainer container, string role, string access, string[] duties)
        {
            container.Border(0.75f).BorderColor(Line).Background(SoftBg).Padding(10).Column(col =>
            {
                col.Spacing(4);
                col.Item().Text(role).Bold().FontColor(Brand).FontSize(11);
                col.Item().Text(access).FontColor(Muted).FontSize(9);
                foreach (var d in duties)
                {
                    col.Item().Row(row =>
                    {
                        row.ConstantItem(12).Text("•").FontColor(Brand);
                        row.RelativeItem().Text(d).FontSize(9.5f);
                    });
                }
            });
        }

        private void WorkflowCard(IContainer container, string title, string[] steps)
        {
            container.Border(0.75f).BorderColor(Line).Padding(10).Column(col =>
            {
                col.Spacing(3);
                col.Item().Text(title).Bold().FontColor(Ink).FontSize(10.5f);
                var i = 0;
                foreach (var s in steps)
                {
                    i++;
                    col.Item().Row(row =>
                    {
                        row.ConstantItem(18).Text($"{i}.").Bold().FontColor(Brand);
                        row.RelativeItem().Text(s).FontSize(9.5f);
                    });
                }
            });
        }

        private void Section(IContainer container, string number, string title)
        {
            container.BorderBottom(1.5f).BorderColor(Brand).PaddingBottom(3).Row(row =>
            {
                row.ConstantItem(26).Text(number).Bold().FontColor(Brand).FontSize(13);
                row.RelativeItem().AlignMiddle().Text(title).Bold().FontColor(BrandDark).FontSize(13);
            });
        }

        private void Bullets(IContainer container, IEnumerable<string> items)
        {
            container.Column(col =>
            {
                col.Spacing(3);
                foreach (var it in items)
                {
                    col.Item().Row(row =>
                    {
                        row.ConstantItem(12).Text("•").FontColor(Brand);
                        row.RelativeItem().Text(it).FontSize(9.5f);
                    });
                }
            });
        }
    }
}
