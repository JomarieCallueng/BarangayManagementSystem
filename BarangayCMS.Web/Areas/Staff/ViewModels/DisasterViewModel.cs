using System;
using System.ComponentModel.DataAnnotations;

namespace BarangayCMS.Areas.Staff.ViewModels
{
    public class DisasterViewModel
    {
        // Ginamit natin ang Id para sa @item.Id link sa Track button
        public int Id { get; set; }

        // Mapped sa @item.IncidentName ng iyong View
        [Required(ErrorMessage = "Ang Pangalan ng Sakuna / Insidente ay kinakailangan.")]
        [MaxLength(150, ErrorMessage = "Hindi pwedeng lumampas sa 150 karakter.")]
        [Display(Name = "Incident / Calamity Name")]
        public string IncidentName { get; set; } = string.Empty;

        // Mapped sa @item.DisasterType
        [Required(ErrorMessage = "Ang Uri ng Sakuna (Disaster Type) ay kinakailangan.")]
        [MaxLength(100, ErrorMessage = "Hindi pwedeng lumampas sa 100 karakter.")]
        [Display(Name = "Disaster / Incident Type")]
        public string DisasterType { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Hindi pwedeng lumampas sa 500 karakter.")]
        [Display(Name = "Situation Description")]
        public string Description { get; set; } = string.Empty;

        [MaxLength(150, ErrorMessage = "Hindi pwedeng lumampas sa 150 karakter.")]
        [Display(Name = "Affected Location / Area")]
        public string Location { get; set; } = string.Empty;

        // Mapped sa @item.OccurrenceDate.ToString(...)
        [Required(ErrorMessage = "Ang Petsa at Oras ay kinakailangan.")]
        [Display(Name = "Date and Time Occurred")]
        public DateTime OccurrenceDate { get; set; } = DateTime.Now;

        // Mapped sa @item.AffectedHouseholdsCount
        [Display(Name = "Affected Households")]
        public int AffectedHouseholdsCount { get; set; } = 0;

        // Mapped sa @item.DisplacedIndividualsCount
        [Display(Name = "Displaced Individuals (Pax)")]
        public int DisplacedIndividualsCount { get; set; } = 0;

        // Mapped sa @item.EvacuationCenterStatus ("Open" / "Closed")
        [MaxLength(50)]
        [Display(Name = "Evacuation Center Status")]
        public string EvacuationCenterStatus { get; set; } = "Closed";

        // Mapped sa @item.ReliefDistributionStatus ("Completed" / "Ongoing")
        [MaxLength(50)]
        [Display(Name = "Relief Distribution Status")]
        public string ReliefDistributionStatus { get; set; } = "Ongoing";

        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "Active";
    }
}