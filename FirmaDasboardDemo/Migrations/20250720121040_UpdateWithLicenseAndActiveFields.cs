using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FirmaDasboardDemo.Migrations
{
    /// <inheritdoc />
    public partial class UpdateWithLicenseAndActiveFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AktifMi",
                table: "Firmalar",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AktifMi",
                table: "FirmaCalisanlari",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LisansSuresiBitis",
                table: "FirmaCalisanlari",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "AktifMi",
                table: "Bayiler",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LisansSuresiBitis",
                table: "Bayiler",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AktifMi",
                table: "Firmalar");

            migrationBuilder.DropColumn(
                name: "AktifMi",
                table: "FirmaCalisanlari");

            migrationBuilder.DropColumn(
                name: "LisansSuresiBitis",
                table: "FirmaCalisanlari");

            migrationBuilder.DropColumn(
                name: "AktifMi",
                table: "Bayiler");

            migrationBuilder.DropColumn(
                name: "LisansSuresiBitis",
                table: "Bayiler");
        }
    }
}
