using System;
using System.ComponentModel.DataAnnotations;

namespace BarangayCMS.Areas.Staff.ViewModels
{
    public class CertificateViewModel
    {
        public int CertificateId { get; set; }

        [Required(ErrorMessage = "Pumili ng residente.")]
        [Display(Name = "Resident Name")]
        public int ResidentId { get; set; }

        public decimal FeePaid { get; set; }
        public string? PaymentReceiptPath { get; set; }


        public string ResidentName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Pumili ng uri ng sertipiko.")]
        [Display(Name = "Certificate Type")]
        public string CertificateType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ibigay ang layunin o dahilan.")]
        public string Purpose { get; set; } = string.Empty;

        [Display(Name = "Control Number")]
        public string ControlNumber { get; set; } = string.Empty;

        [Display(Name = "Fee Paid")]
        public decimal AmountPaid { get; set; }

        [Display(Name = "O.R. Number")]
        public string OfficialReceiptNumber { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; } = "Pending";

        [Display(Name = "Date Requested")]
        public DateTime DateRequested { get; set; } = DateTime.Now;

        [Display(Name = "Date Issued")]
        public DateTime? DateIssued { get; set; }

        [Display(Name = "Issued By")]
        public string IssuedBy { get; set; } = string.Empty;
    }
}