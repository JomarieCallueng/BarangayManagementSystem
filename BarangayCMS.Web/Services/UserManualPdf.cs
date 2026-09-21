using System;
using QuestPDF.Fluent;

namespace BarangayCMS.Web.Services
{
    // Bumubuo (isang beses) at nagka-cache ng User Manual PDF bytes. Static ang
    // nilalaman kaya sapat nang i-generate minsan at i-reuse — walang duplicate file
    // na sine-save sa disk. Reused ng Admin at Staff HelpController.Manual().
    public static class UserManualPdf
    {
        public const string FileName = "User-Manual.pdf";
        public const string ContentType = "application/pdf";

        private static readonly Lazy<byte[]> _bytes =
            new Lazy<byte[]>(() =>
            {
                // Safety net: kung may isang glyph na wala sa font (hal. mga espesyal
                // na simbolo), huwag mag-throw ng DocumentDrawingException — i-render na
                // lang ang PDF. Iniiwas nito ang pag-crash ng download sa anumang
                // environment na kulang sa ilang glyph.
                QuestPDF.Settings.CheckIfAllTextGlyphsAreAvailable = false;
                return new UserManualDocument().GeneratePdf();
            }, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

        public static byte[] GetBytes() => _bytes.Value;
    }
}
