using System;

namespace BarangayCMS.DTO
{
    public class CertificateDTO
    {
        public int Id { get; set; }
        public int ResidentId { get; set; }
        public string ResidentName { get; set; } = string.Empty; // Para madaling ipakita sa table views

        public string CertificateType { get; set; } = string.Empty; // Clearance, Indigency, Residency, Business Permit
        public string Purpose { get; set; } = string.Empty;
        public string ControlNumber { get; set; } = string.Empty; // Brgy Tracking Code

        public decimal FeePaid { get; set; }
        public string OfficialReceiptNumber { get; set; } = string.Empty;

        public string Status { get; set; } = "Pending"; // Pending, Approved, Issued, Rejected
        public DateTime IssuedDate { get; set; }
        public string IssuedBy { get; set; } = string.Empty;

     
        public string? PaymentReceiptPath { get; set; }

        public byte[]? PaymentReceiptBytes { get; set; }
       
    }
}
