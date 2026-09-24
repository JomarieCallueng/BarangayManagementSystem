using System.Text;
using System.Text.RegularExpressions;

namespace BarangayCMS.Web.Validation
{
    /// <summary>
    /// Normalizes person-name text: trims, collapses repeated spaces, and applies
    /// proper capitalization while preserving hyphens and apostrophes.
    /// "jomarie" → "Jomarie", "juan dela cruz" → "Juan Dela Cruz",
    /// "mary-jane" → "Mary-Jane", "o'connor" → "O'Connor".
    /// Only for person-name fields — never for emails, usernames, or free text.
    /// </summary>
    public static class NameFormatter
    {
        public static string? ToProperName(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;

            // Trim + collapse internal whitespace runs to a single space.
            var s = Regex.Replace(input.Trim(), @"\s+", " ").ToLowerInvariant();

            var sb = new StringBuilder(s.Length);
            bool capNext = true;
            foreach (var c in s)
            {
                if (c == ' ' || c == '-' || c == '\'')
                {
                    sb.Append(c);
                    capNext = true;
                }
                else
                {
                    sb.Append(capNext ? char.ToUpperInvariant(c) : c);
                    capNext = false;
                }
            }
            return sb.ToString();
        }
    }
}
