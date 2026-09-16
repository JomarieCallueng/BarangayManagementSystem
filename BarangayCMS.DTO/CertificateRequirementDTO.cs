namespace BarangayCMS.DTO
{
    public class CertificateRequirementDTO
    {
        public int Id { get; set; }
        public int CertificateTypeId { get; set; }
        public string RequirementName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsRequired { get; set; } = true;
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
