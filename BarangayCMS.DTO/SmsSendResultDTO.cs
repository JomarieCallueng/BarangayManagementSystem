using System.Collections.Generic;

namespace BarangayCMS.DTO
{
    /// <summary>
    /// Resulta ng isang bulk Emergency SMS blast gamit ang Semaphore API.
    /// </summary>
    public class SmsSendResultDTO
    {
        // Bilang ng valid na numero na sinubukang padalhan
        public int TotalRecipients { get; set; }

        // Bilang ng matagumpay na naipadala
        public int SuccessCount { get; set; }

        // Bilang ng nabigo (API failure)
        public int FailedCount { get; set; }

        // Mga numerong invalid ang format (hindi kasama sa pagpapadala)
        public List<string> InvalidNumbers { get; set; } = new List<string>();

        // Mga numerong nabigong maipadala kahit valid ang format
        public List<string> FailedNumbers { get; set; } = new List<string>();

        // Kabuuang status label: Sent, Partial, Failed
        public string Status
        {
            get
            {
                if (TotalRecipients == 0) return "Failed";
                if (SuccessCount == 0) return "Failed";
                if (FailedCount > 0) return "Partial";
                return "Sent";
            }
        }
    }
}
