using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FirmaDasboardDemo.Migrations
{
    /// <inheritdoc />
    public partial class BayiMesajOlustur : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BayiMesajlar_Bayiler_BayiId",
                table: "BayiMesajlar");

            migrationBuilder.DropForeignKey(
                name: "FK_BayiMesajlar_Urun_UrunId",
                table: "BayiMesajlar");

            migrationBuilder.AddColumn<string>(
                name: "BayiCevabi",
                table: "BayiMesajlar",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "BayiCevapTarihi",
                table: "BayiMesajlar",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FirmaCevapTarihi",
                table: "BayiMesajlar",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BayiMesajlar_FirmaId",
                table: "BayiMesajlar",
                column: "FirmaId");

            migrationBuilder.AddForeignKey(
                name: "FK_BayiMesajlar_Bayiler_BayiId",
                table: "BayiMesajlar",
                column: "BayiId",
                principalTable: "Bayiler",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BayiMesajlar_Firmalar_FirmaId",
                table: "BayiMesajlar",
                column: "FirmaId",
                principalTable: "Firmalar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BayiMesajlar_Urun_UrunId",
                table: "BayiMesajlar",
                column: "UrunId",
                principalTable: "Urun",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BayiMesajlar_Bayiler_BayiId",
                table: "BayiMesajlar");

            migrationBuilder.DropForeignKey(
                name: "FK_BayiMesajlar_Firmalar_FirmaId",
                table: "BayiMesajlar");

            migrationBuilder.DropForeignKey(
                name: "FK_BayiMesajlar_Urun_UrunId",
                table: "BayiMesajlar");

            migrationBuilder.DropIndex(
                name: "IX_BayiMesajlar_FirmaId",
                table: "BayiMesajlar");

            migrationBuilder.DropColumn(
                name: "BayiCevabi",
                table: "BayiMesajlar");

            migrationBuilder.DropColumn(
                name: "BayiCevapTarihi",
                table: "BayiMesajlar");

            migrationBuilder.DropColumn(
                name: "FirmaCevapTarihi",
                table: "BayiMesajlar");

            migrationBuilder.AddForeignKey(
                name: "FK_BayiMesajlar_Bayiler_BayiId",
                table: "BayiMesajlar",
                column: "BayiId",
                principalTable: "Bayiler",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BayiMesajlar_Urun_UrunId",
                table: "BayiMesajlar",
                column: "UrunId",
                principalTable: "Urun",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
