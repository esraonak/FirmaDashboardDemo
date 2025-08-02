using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FirmaDasboardDemo.Migrations
{
    /// <inheritdoc />
    public partial class KvkkEtkAlanlari : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EtkOnaylandiMi",
                table: "FirmaCalisanlari",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "KvkkOnaylandiMi",
                table: "FirmaCalisanlari",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EtkOnaylandiMi",
                table: "Bayiler",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "KvkkOnaylandiMi",
                table: "Bayiler",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EtkOnaylandiMi",
                table: "FirmaCalisanlari");

            migrationBuilder.DropColumn(
                name: "KvkkOnaylandiMi",
                table: "FirmaCalisanlari");

            migrationBuilder.DropColumn(
                name: "EtkOnaylandiMi",
                table: "Bayiler");

            migrationBuilder.DropColumn(
                name: "KvkkOnaylandiMi",
                table: "Bayiler");
        }
    }
}
