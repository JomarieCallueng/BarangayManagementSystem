using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarangayCMS.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentFieldsToCertificate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentReceiptPath",
                table: "Certificates",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentReceiptPath",
                table: "Certificates");
        }
    }
}
