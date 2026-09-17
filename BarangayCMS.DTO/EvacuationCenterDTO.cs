using System;

namespace BarangayCMS.DTO
{
    public class EvacuationCenterDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string SitioPurok { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public int CurrentOccupants { get; set; }
        public string ContactNumber { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public bool IsActive { get; set; }
        public DateTime? DateUpdated { get; set; }

        // ---- Computed (business logic sa BLL) ----

        // Available slots = Capacity - CurrentOccupants (hindi bababa sa 0).
        public int AvailableSlots => Capacity <= 0 ? 0 : Math.Max(0, Capacity - CurrentOccupants);

        // Porsyento ng okupasyon (0-100) para sa progress bar.
        public int OccupancyPercent =>
            Capacity <= 0 ? 0 : Math.Min(100, (int)Math.Round((double)CurrentOccupants / Capacity * 100));

        // May tunay bang capacity data? Kapag wala, iiwasan ang pag-imbento ng bilang.
        public bool HasCapacityData => Capacity > 0;

        // Occupancy status: OPEN / NEAR CAPACITY / FULL / CLOSED
        // Sinusunod ang IsActive muna, pagkatapos ang okupasyon.
        public string OccupancyStatus
        {
            get
            {
                if (!IsActive) return "CLOSED";
                if (!HasCapacityData) return "OPEN";
                if (CurrentOccupants >= Capacity) return "FULL";
                if (OccupancyPercent >= 90) return "NEAR CAPACITY";
                return "OPEN";
            }
        }
    }
}
