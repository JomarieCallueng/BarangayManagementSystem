using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarangayCMS.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddCertificatedataTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "TemplateData",
                table: "CertificateTypes",
                type: "varbinary(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TemplateData",
                table: "CertificateTypes");
        }
    }
}
