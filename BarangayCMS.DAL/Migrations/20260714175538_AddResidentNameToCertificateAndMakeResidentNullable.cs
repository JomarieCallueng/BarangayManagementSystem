using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarangayCMS.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddResidentNameToCertificateAndMakeResidentNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Certificates_Residents_ResidentId",
                table: "Certificates");

            migrationBuilder.AlterColumn<int>(
                name: "ResidentId",
                table: "Certificates",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "ResidentName",
                table: "Certificates",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Certificates_Residents_ResidentId",
                table: "Certificates",
                column: "ResidentId",
                principalTable: "Residents",
                principalColumn: "ResidentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Certificates_Residents_ResidentId",
                table: "Certificates");

            migrationBuilder.DropColumn(
                name: "ResidentName",
                table: "Certificates");

            migrationBuilder.AlterColumn<int>(
                name: "ResidentId",
                table: "Certificates",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Certificates_Residents_ResidentId",
                table: "Certificates",
                column: "ResidentId",
                principalTable: "Residents",
                principalColumn: "ResidentId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
