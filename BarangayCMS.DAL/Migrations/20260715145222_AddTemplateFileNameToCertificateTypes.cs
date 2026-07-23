using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarangayCMS.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddTemplateFileNameToCertificateTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TemplateFileName",
                table: "CertificateTypes",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TemplateFileName",
                table: "CertificateTypes");
        }
    }
}
