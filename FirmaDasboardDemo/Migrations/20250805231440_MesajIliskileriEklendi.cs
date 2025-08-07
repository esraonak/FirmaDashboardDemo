using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FirmaDasboardDemo.Migrations
{
    /// <inheritdoc />
    public partial class MesajIliskileriEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BayiCevabi",
                table: "BayiMesajlar");

            migrationBuilder.DropColumn(
                name: "BayiCevapTarihi",
                table: "BayiMesajlar");

            migrationBuilder.DropColumn(
                name: "FirmaCevabi",
                table: "BayiMesajlar");

            migrationBuilder.DropColumn(
                name: "FirmaCevapTarihi",
                table: "BayiMesajlar");

            migrationBuilder.DropColumn(
                name: "MesajIcerik",
                table: "BayiMesajlar");

            migrationBuilder.CreateTable(
                name: "MesajSatirlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BayiMesajId = table.Column<int>(type: "int", nullable: false),
                    BayiId = table.Column<int>(type: "int", nullable: false),
                    FirmaId = table.Column<int>(type: "int", nullable: false),
                    UrunId = table.Column<int>(type: "int", nullable: true),
                    Icerik = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GonderenFirmaMi = table.Column<bool>(type: "bit", nullable: false),
                    Tarih = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OkunduMu = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MesajSatirlari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MesajSatirlari_BayiMesajlar_BayiMesajId",
                        column: x => x.BayiMesajId,
                        principalTable: "BayiMesajlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MesajSatirlari_Bayiler_BayiId",
                        column: x => x.BayiId,
                        principalTable: "Bayiler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MesajSatirlari_Firmalar_FirmaId",
                        column: x => x.FirmaId,
                        principalTable: "Firmalar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MesajSatirlari_Urun_UrunId",
                        column: x => x.UrunId,
                        principalTable: "Urun",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MesajSatirlari_BayiId",
                table: "MesajSatirlari",
                column: "BayiId");

            migrationBuilder.CreateIndex(
                name: "IX_MesajSatirlari_BayiMesajId",
                table: "MesajSatirlari",
                column: "BayiMesajId");

            migrationBuilder.CreateIndex(
                name: "IX_MesajSatirlari_FirmaId",
                table: "MesajSatirlari",
                column: "FirmaId");

            migrationBuilder.CreateIndex(
                name: "IX_MesajSatirlari_UrunId",
                table: "MesajSatirlari",
                column: "UrunId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MesajSatirlari");

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

            migrationBuilder.AddColumn<string>(
                name: "FirmaCevabi",
                table: "BayiMesajlar",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "FirmaCevapTarihi",
                table: "BayiMesajlar",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MesajIcerik",
                table: "BayiMesajlar",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
