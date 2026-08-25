using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BarangayCMS.Web.Areas.Admin.Models
{
    public class CertificateViewModel
    {
        public int Id { get; set; }

        // Pwedeng nullable para pwedeng mag-approve kahit walang account ang residente
        [Display(Name = "Resident")]
        public int? ResidentId { get; set; }

        [Display(Name = "Resident Full Name")]
        public string ResidentFullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Certificate type is required.")]
        [Display(Name = "Certificate Type")]
        public string CertificateType { get; set; } = string.Empty;

        // 🌟 DAGDAG: Purpose property para sa dahilan ng pagkuha
        [Required(ErrorMessage = "Dahilan ng pagkuha is required.")]
        [Display(Name = "Purpose of Request")]
        public string Purpose { get; set; } = string.Empty;

        // 🌟 DAGDAG: Control Number ng sertipiko
        [Display(Name = "Control Number")]
        public string? ControlNumber { get; set; }

        [Required]
        [Display(Name = "Fee Paid")]
        public decimal FeePaid { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date Requested")]
        public DateTime DateRequested { get; set; } = DateTime.Now;

        [DataType(DataType.Date)]
        [Display(Name = "Date Issued")]
        public DateTime? DateIssued { get; set; }

        // 🌟 DAGDAG: Kung sino ang nag-issue (hal. Punong Barangay / Staff)
        [Display(Name = "Issued By")]
        public string? IssuedBy { get; set; }

        public string? PaymentReceiptPath { get; set; }

        [Required]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Issued, Cancelled

        public IEnumerable<SelectListItem>? ResidentList { get; set; }
    }
}