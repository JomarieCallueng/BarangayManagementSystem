using System;

namespace BarangayCMS.DTO
{
    public class CertificateDTO
    {
        public int Id { get; set; }
        public int ResidentId { get; set; }
        public string ResidentName { get; set; } = string.Empty;

        // 🟢 IDAGDAG ITO PARA SA PUROK:
        public string Purok { get; set; } = string.Empty;

        public string CertificateType { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
        public string ControlNumber { get; set; } = string.Empty;

        public decimal FeePaid { get; set; }
        public string OfficialReceiptNumber { get; set; } = string.Empty;

        public string Status { get; set; } = "Pending";
        public DateTime IssuedDate { get; set; }
        public string IssuedBy { get; set; } = string.Empty;

        public string? PaymentReceiptPath { get; set; }
    }
}