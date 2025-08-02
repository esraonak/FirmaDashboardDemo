using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FirmaDasboardDemo.Migrations
{
    /// <inheritdoc />
    public partial class AddDegiskenIdToFirmaDegiskenUrun : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DegiskenId",
                table: "FirmaDegiskenUrunler",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FirmaDegiskenUrunId",
                table: "FirmaDegiskenUrunler",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FirmaDegiskenUrunler_FirmaDegiskenUrunId",
                table: "FirmaDegiskenUrunler",
                column: "FirmaDegiskenUrunId");

            migrationBuilder.AddForeignKey(
                name: "FK_FirmaDegiskenUrunler_FirmaDegiskenUrunler_FirmaDegiskenUrunId",
                table: "FirmaDegiskenUrunler",
                column: "FirmaDegiskenUrunId",
                principalTable: "FirmaDegiskenUrunler",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FirmaDegiskenUrunler_FirmaDegiskenUrunler_FirmaDegiskenUrunId",
                table: "FirmaDegiskenUrunler");

            migrationBuilder.DropIndex(
                name: "IX_FirmaDegiskenUrunler_FirmaDegiskenUrunId",
                table: "FirmaDegiskenUrunler");

            migrationBuilder.DropColumn(
                name: "DegiskenId",
                table: "FirmaDegiskenUrunler");

            migrationBuilder.DropColumn(
                name: "FirmaDegiskenUrunId",
                table: "FirmaDegiskenUrunler");
        }
    }
}
