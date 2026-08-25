using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BarangayCMS.Entities
{
    public class CertificateType
    {
        [Key]
        public int CertificateTypeId { get; set; } // Primary Key

        [Required]
        [Display(Name = "Pangalan ng Sertipiko")]
        public string CertificateName { get; set; } = string.Empty; // e.g., Barangay Clearance

        [Required]
        [Column(TypeName = "decimal(18, 2)")] // Para saktong pera ang format
        [Display(Name = "Presyo (₱)")]
        public decimal Price { get; set; } // e.g., 50.00

        public string? TemplateFileName { get; set; }
        public byte[]? TemplateData { get; set; }
    }
}