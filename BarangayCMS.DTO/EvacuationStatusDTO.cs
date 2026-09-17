using System;
using System.Collections.Generic;

namespace BarangayCMS.DTO
{
    /// <summary>
    /// Buong-barangay na kalagayan ng paglikas + mga tagubilin + emergency contacts.
    /// </summary>
    public class EvacuationStatusDTO
    {
        public int Id { get; set; }
        public string OverallStatus { get; set; } = "Normal";
        public string? StatusMessage { get; set; }

        // Raw (multiline) na anyo — ginagamit ng Admin edit form.
        public string Instructions { get; set; } = string.Empty;
        public string EmergencyContacts { get; set; } = string.Empty;

        public string? UpdatedBy { get; set; }
        public DateTime DateUpdated { get; set; }

        // Na-normalize na value para sa lohika/badge: NORMAL / PREPARED / ACTIVE
        public string NormalizedStatus => (OverallStatus ?? "Normal").Trim().ToUpperInvariant();
    }

    /// <summary>
    /// Isang naka-parse na emergency contact (Label + Number).
    /// </summary>
    public class EmergencyContactDTO
    {
        public string Label { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
    }

    /// <summary>
    /// Aggregate na model na ibinibigay sa Public Evacuation view — isang beses
    /// na pagkuha ng lahat ng kailangan: status, active centers, mga tagubilin,
    /// emergency contacts, at ang huling pag-update.
    /// </summary>
    public class PublicEvacuationDTO
    {
        public EvacuationStatusDTO Status { get; set; } = new EvacuationStatusDTO();
        public List<EvacuationCenterDTO> ActiveCenters { get; set; } = new List<EvacuationCenterDTO>();
        public List<string> Instructions { get; set; } = new List<string>();
        public List<EmergencyContactDTO> EmergencyContacts { get; set; } = new List<EmergencyContactDTO>();

        // Pinakabagong timestamp mula sa status o sa mga center — kung alin ang huli.
        public DateTime LastUpdated { get; set; }

        // Kabuuang bilang ng available na slot sa lahat ng active center.
        public int TotalAvailableSlots { get; set; }
        public int TotalCapacity { get; set; }
        public int TotalOccupants { get; set; }
    }
}
