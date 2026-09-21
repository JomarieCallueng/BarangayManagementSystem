using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace BarangayCMS.Web.Areas.Admin.Services
{
    // Ang mga "slot" na pinagbabatayan ng structure validation ng Sangguniang
    // Barangay at Sangguniang Kabataan.
    public enum OfficialSlot
    {
        Captain,
        BarangayKagawad,
        BarangaySecretary,
        BarangayTreasurer,
        SkChair,
        SkKagawad,
        SkSecretary,
        SkTreasurer,
        Other
    }

    // Isang row sa summary (used/limit) ng isang posisyon.
    public class SlotStatus
    {
        public string Label { get; set; } = string.Empty;
        public int Used { get; set; }
        public int Limit { get; set; }
        public bool Complete => Used >= Limit;
        public bool Over => Used > Limit;
    }

    public class StructureSummary
    {
        public List<SlotStatus> Barangay { get; } = new();
        public List<SlotStatus> SK { get; } = new();
    }

    /// <summary>
    /// Sentralisadong panuntunan para sa tamang bilang at uri ng mga opisyal.
    /// Ginagamit ng Admin controller (server-side na huling awtoridad), ng Admin
    /// forms (UI hints/disabling), at ng summary panel. Walang hardcoded na pangalan —
    /// posisyon lamang ang batayan.
    /// </summary>
    public static class OfficialStructure
    {
        // Kanonikal na listahan ng posisyon na inaalok sa Admin dropdown.
        public static readonly (string Value, string Label, OfficialSlot Slot)[] Options = new[]
        {
            ("Barangay Captain",       "Barangay Captain (Punong Barangay)",      OfficialSlot.Captain),
            ("Barangay Kagawad",       "Barangay Kagawad",                        OfficialSlot.BarangayKagawad),
            ("Barangay Secretary",     "Barangay Secretary",                      OfficialSlot.BarangaySecretary),
            ("Barangay Treasurer",     "Barangay Treasurer",                      OfficialSlot.BarangayTreasurer),
            ("SK Chairman",            "SK Chairperson (Ex-Officio sa Barangay)", OfficialSlot.SkChair),
            ("SK Kagawad",             "SK Kagawad",                              OfficialSlot.SkKagawad),
            ("SK Secretary",           "SK Secretary",                            OfficialSlot.SkSecretary),
            ("SK Treasurer",           "SK Treasurer",                            OfficialSlot.SkTreasurer),
            ("Barangay Tanod (Chief)", "Barangay Tanod (Chief)",                  OfficialSlot.Other),
        };

        public static bool IsSk(string? position)
        {
            var p = (position ?? string.Empty).ToLowerInvariant();
            return p.Contains("sk ") || p.StartsWith("sk") || p.Contains("kabataan") || p.Contains("youth");
        }

        // Normalisadong pangalan para sa duplicate check: trim, i-collapse ang sunod-sunod
        // na espasyo, at case-insensitive.
        public static string NormalizeName(string? name)
        {
            var n = (name ?? string.Empty).Trim();
            n = Regex.Replace(n, @"\s+", " ");
            return n.ToLowerInvariant();
        }

        // true kung ang slot ay isang Kagawad (may committee-uniqueness rule).
        public static bool IsKagawadSlot(OfficialSlot slot)
            => slot == OfficialSlot.BarangayKagawad || slot == OfficialSlot.SkKagawad;

        // Iuuri ang isang (posibleng free-text) na posisyon sa isang slot.
        public static OfficialSlot Classify(string? position)
        {
            var p = (position ?? string.Empty).ToLowerInvariant();
            if (IsSk(position))
            {
                if (p.Contains("chair")) return OfficialSlot.SkChair;
                if (p.Contains("secretary")) return OfficialSlot.SkSecretary;
                if (p.Contains("treasurer")) return OfficialSlot.SkTreasurer;
                if (p.Contains("kagawad") || p.Contains("member") || p.Contains("councilor")) return OfficialSlot.SkKagawad;
                return OfficialSlot.Other;
            }
            if (p.Contains("captain") || p.Contains("punong")) return OfficialSlot.Captain;
            if (p.Contains("secretary")) return OfficialSlot.BarangaySecretary;
            if (p.Contains("treasurer")) return OfficialSlot.BarangayTreasurer;
            if (p.Contains("kagawad") || p.Contains("councilor") || p.Contains("konsehal")) return OfficialSlot.BarangayKagawad;
            return OfficialSlot.Other;
        }

        // int.MaxValue = walang limitasyon (hal. Tanod).
        public static int Limit(OfficialSlot slot) => slot switch
        {
            OfficialSlot.Captain => 1,
            OfficialSlot.BarangaySecretary => 1,
            OfficialSlot.BarangayTreasurer => 1,
            OfficialSlot.SkChair => 1,
            OfficialSlot.SkSecretary => 1,
            OfficialSlot.SkTreasurer => 1,
            OfficialSlot.BarangayKagawad => 7,
            OfficialSlot.SkKagawad => 7,
            _ => int.MaxValue
        };

        // Ang committee na awtomatikong itinatalaga sa SK Chairperson (Ex-Officio).
        public const string SkChairCommittee = "Committee on Youth and Sports Development";

        // Kanonikal na standing committees (katugma ng public "Standing Committees" sections).
        public static readonly string[] BarangayCommittees =
        {
            "Appropriations & Finance",
            "Peace and Order & Public Safety",
            "Health and Sanitation",
            "Infrastructure & Public Works",
            "Education & Culture",
            "Environment & Agriculture",
            "Women, Family & Social Services",
            "Youth & Sports Development",
        };

        public static readonly string[] SkCommittees =
        {
            "Education and Culture",
            "Environmental Protection, Climate Change & DRRM",
            "Youth Employment and Livelihood",
            "Health, Health Services & Anti-Drug Abuse",
            "Gender and Development",
            "Sports Development",
            "Active Citizenship & Capability Building",
        };

        // Kung paano hinahawakan ang Committee field para sa isang slot:
        //  none  = walang tiyak na komite (Captain, Secretary, Treasurer)
        //  fixed = awtomatiko/naka-lock (SK Chairperson)
        //  barangay / sk = pinipili mula sa standing committees (Kagawad)
        //  free  = malayang komite (iba pa, hal. Tanod)
        public static string CommitteeMode(OfficialSlot slot) => slot switch
        {
            OfficialSlot.Captain => "none",
            OfficialSlot.BarangaySecretary => "none",
            OfficialSlot.BarangayTreasurer => "none",
            OfficialSlot.SkSecretary => "none",
            OfficialSlot.SkTreasurer => "none",
            OfficialSlot.SkChair => "fixed",
            OfficialSlot.BarangayKagawad => "barangay",
            OfficialSlot.SkKagawad => "sk",
            _ => "none" // Tanod (Chief) at iba pa — walang tiyak na komite
        };

        /// <summary>
        /// Role-based na Committee value — ito ang server-side na panghuling awtoridad.
        /// Punong Barangay / Secretary / Treasurer => walang komite (empty).
        /// SK Chairperson => sapilitang "@SkChairCommittee".
        /// Kagawad / iba pa => ang piniling committee (pinananatili kung meron na).
        /// </summary>
        public static string NormalizeCommittee(string? position, string? provided)
        {
            return CommitteeMode(Classify(position)) switch
            {
                "none" => string.Empty,
                "fixed" => SkChairCommittee,
                _ => (provided ?? string.Empty).Trim()
            };
        }

        // Kung ano ang ipapakita sa publiko (parehong panuntunan gaya ng normalization),
        // kaya kahit lumang stored value (hal. "Executive Committee" sa Captain) ay
        // hindi na lalabas.
        public static string DisplayCommittee(string? position, string? stored)
            => NormalizeCommittee(position, stored);

        public static string SlotLabel(OfficialSlot slot) => slot switch
        {
            OfficialSlot.Captain => "Barangay Captain",
            OfficialSlot.BarangayKagawad => "Barangay Kagawad",
            OfficialSlot.BarangaySecretary => "Barangay Secretary",
            OfficialSlot.BarangayTreasurer => "Barangay Treasurer",
            OfficialSlot.SkChair => "SK Chairperson",
            OfficialSlot.SkKagawad => "SK Kagawad",
            OfficialSlot.SkSecretary => "SK Secretary",
            OfficialSlot.SkTreasurer => "SK Treasurer",
            _ => "Official"
        };

        public static Dictionary<OfficialSlot, int> CountBySlot(IEnumerable<string> activePositions)
        {
            var dict = new Dictionary<OfficialSlot, int>();
            foreach (var pos in activePositions)
            {
                var s = Classify(pos);
                dict[s] = dict.TryGetValue(s, out var c) ? c + 1 : 1;
            }
            return dict;
        }

        /// <summary>
        /// Nagbabalik ng error message kung puno na ang slot para sa posisyong ito
        /// (base sa listahan ng ibang aktibong posisyon), o null kung pwede pa.
        /// Ito ang server-side na panghuling awtoridad.
        /// </summary>
        public static string? ValidateSlot(string? position, IEnumerable<string> otherActivePositions)
        {
            var slot = Classify(position);
            var limit = Limit(slot);
            if (limit == int.MaxValue) return null;

            var count = otherActivePositions.Count(p => Classify(p) == slot);
            if (count >= limit)
            {
                return limit == 1
                    ? $"May aktibong {SlotLabel(slot)} na. Isa lamang ang pinapayagan sa posisyong ito — i-deactivate muna ang kasalukuyan bago magdagdag ng bago."
                    : $"Puno na ang {SlotLabel(slot)} ({count}/{limit}). Hindi na pwedeng magdagdag pa — i-deactivate muna ang isa bago magdagdag.";
            }
            return null;
        }

        public static StructureSummary BuildSummary(IEnumerable<string> activePositions)
        {
            var counts = CountBySlot(activePositions);
            int C(OfficialSlot s) => counts.TryGetValue(s, out var v) ? v : 0;

            var summary = new StructureSummary();
            summary.Barangay.Add(new SlotStatus { Label = "Punong Barangay / Captain", Used = C(OfficialSlot.Captain), Limit = 1 });
            summary.Barangay.Add(new SlotStatus { Label = "Barangay Kagawad", Used = C(OfficialSlot.BarangayKagawad), Limit = 7 });
            summary.Barangay.Add(new SlotStatus { Label = "SK Chairperson (Ex-Officio)", Used = C(OfficialSlot.SkChair), Limit = 1 });
            summary.Barangay.Add(new SlotStatus { Label = "Barangay Secretary", Used = C(OfficialSlot.BarangaySecretary), Limit = 1 });
            summary.Barangay.Add(new SlotStatus { Label = "Barangay Treasurer", Used = C(OfficialSlot.BarangayTreasurer), Limit = 1 });

            summary.SK.Add(new SlotStatus { Label = "SK Chairperson", Used = C(OfficialSlot.SkChair), Limit = 1 });
            summary.SK.Add(new SlotStatus { Label = "SK Kagawad", Used = C(OfficialSlot.SkKagawad), Limit = 7 });
            summary.SK.Add(new SlotStatus { Label = "SK Secretary", Used = C(OfficialSlot.SkSecretary), Limit = 1 });
            summary.SK.Add(new SlotStatus { Label = "SK Treasurer", Used = C(OfficialSlot.SkTreasurer), Limit = 1 });
            return summary;
        }
    }
}
