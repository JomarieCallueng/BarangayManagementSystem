using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BarangayCMS.Web.Areas.Admin.Models
{
    public class CertificateViewModel
    {
        public int Id { get; set; }

        // 🌟 TANGGALIN ANG [Required] AT GAWING NULLABLE (int?) 
        // para pwedeng mag-approve kahit walang account o ResidentId ang nag-request!
        [Display(Name = "Resident")]
        public int? ResidentId { get; set; }

        [Display(Name = "Resident Full Name")]
        public string ResidentFullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Certificate type is required.")]
        [Display(Name = "Certificate Type")]
        public string CertificateType { get; set; } = string.Empty;

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

       

        public string? PaymentReceiptPath { get; set; }

        [Required]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Issued, Cancelled

        public IEnumerable<SelectListItem>? ResidentList { get; set; }
    }
}