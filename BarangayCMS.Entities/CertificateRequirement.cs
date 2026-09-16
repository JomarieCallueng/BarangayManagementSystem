using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BarangayCMS.Entities
{
    // A single document/requirement na kailangan (o opsyonal) para sa isang
    // uri ng sertipiko. Isang CertificateType → maraming CertificateRequirement.
    public class CertificateRequirement
    {
        [Key]
        public int CertificateRequirementId { get; set; }

        // Foreign key papunta sa CertificateType.
        public int CertificateTypeId { get; set; }

        [Required]
        [Display(Name = "Requirement")]
        public string RequirementName { get; set; } = string.Empty; // hal. "Valid Government ID"

        [Display(Name = "Paglalarawan")]
        public string? Description { get; set; } // opsyonal na dagdag na paliwanag

        // true = Required, false = Optional
        [Display(Name = "Required?")]
        public bool IsRequired { get; set; } = true;

        // Pagkakasunod-sunod ng pagpapakita sa listahan.
        [Display(Name = "Order")]
        public int DisplayOrder { get; set; } = 0;

        // Pang-soft toggle: kung false, hindi ipapakita sa request form.
        public bool IsActive { get; set; } = true;

        [ForeignKey(nameof(CertificateTypeId))]
        public virtual CertificateType? CertificateType { get; set; }
    }
}
