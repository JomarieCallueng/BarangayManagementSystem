using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarangayCMS.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddCommitteesAndAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Committees",
                columns: table => new
                {
                    CommitteeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IconClass = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Committees", x => x.CommitteeId);
                });

            migrationBuilder.CreateTable(
                name: "CommitteeAssignments",
                columns: table => new
                {
                    CommitteeAssignmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CommitteeId = table.Column<int>(type: "int", nullable: false),
                    BarangayOfficialId = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AssignedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommitteeAssignments", x => x.CommitteeAssignmentId);
                    table.ForeignKey(
                        name: "FK_CommitteeAssignments_BarangayOfficials_BarangayOfficialId",
                        column: x => x.BarangayOfficialId,
                        principalTable: "BarangayOfficials",
                        principalColumn: "BarangayOfficialId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommitteeAssignments_Committees_CommitteeId",
                        column: x => x.CommitteeId,
                        principalTable: "Committees",
                        principalColumn: "CommitteeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommitteeAssignments_BarangayOfficialId",
                table: "CommitteeAssignments",
                column: "BarangayOfficialId");

            migrationBuilder.CreateIndex(
                name: "IX_CommitteeAssignments_CommitteeId",
                table: "CommitteeAssignments",
                column: "CommitteeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommitteeAssignments");

            migrationBuilder.DropTable(
                name: "Committees");
        }
    }
}
