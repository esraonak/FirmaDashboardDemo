using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FirmaDasboardDemo.Migrations
{
    /// <inheritdoc />
    public partial class InitialSetupWithLicensing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LisansBitisTarihi",
                table: "Firmalar",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "MaxBayiSayisi",
                table: "Firmalar",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxCalisanSayisi",
                table: "Firmalar",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LisansBitisTarihi",
                table: "Firmalar");

            migrationBuilder.DropColumn(
                name: "MaxBayiSayisi",
                table: "Firmalar");

            migrationBuilder.DropColumn(
                name: "MaxCalisanSayisi",
                table: "Firmalar");
        }
    }
}
