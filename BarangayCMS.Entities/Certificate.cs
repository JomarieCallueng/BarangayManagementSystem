using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BarangayCMS.Entities
{
    public class Certificate
    {
        [Key]
        public int CertificateId { get; set; }

        // 1. Ginawa nating nullable (int?) para pwedeng walang account/ID ang humihingi mula sa labas
        public int? ResidentId { get; set; }

        // 🌟 2. DINAGDAG DITO: Ang pangalan ng aplikante na tinype sa labas
        [Required]
        public string ResidentName { get; set; } = string.Empty;

        [Required]
        public string CertificateType { get; set; } = string.Empty; // Clearance, Indigency, atbp.

        public string Purpose { get; set; } = string.Empty;
        public string ControlNumber { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal FeePaid { get; set; }
        public string OfficialReceiptNumber { get; set; } = string.Empty;

        public string Status { get; set; } = "Pending"; // Pending, Approved, Issued, Rejected

        public DateTime DateRequested { get; set; }
        public DateTime? DateIssued { get; set; }
        public string IssuedBy { get; set; } = string.Empty;
        public string? PaymentReceiptPath { get; set; }

       

        // 🌟 3. ITINAMA DITO: Ginawa nating nullable (?) ang Resident navigation property 
        // para payagan ng Entity Framework na mag-save kahit walang login account o walang ResidentId.
        [ForeignKey("ResidentId")]
        public virtual Resident? Resident { get; set; }
    }
}