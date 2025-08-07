using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FirmaDasboardDemo.Migrations
{
    /// <inheritdoc />
    public partial class AddBayiMesajTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BayiMesajlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BayiId = table.Column<int>(type: "int", nullable: false),
                    FirmaId = table.Column<int>(type: "int", nullable: false),
                    UrunId = table.Column<int>(type: "int", nullable: false),
                    Tarih = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MesajIcerik = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GirilenHucrelerJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GorunenHucrelerJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SatisFiyatlariJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirmaCevabi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AktifMi = table.Column<bool>(type: "bit", nullable: false),
                    FirmaGoruntulediMi = table.Column<bool>(type: "bit", nullable: false),
                    BayiGoruntulediMi = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BayiMesajlar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BayiMesajlar_Bayiler_BayiId",
                        column: x => x.BayiId,
                        principalTable: "Bayiler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BayiMesajlar_Urun_UrunId",
                        column: x => x.UrunId,
                        principalTable: "Urun",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BayiMesajlar_BayiId",
                table: "BayiMesajlar",
                column: "BayiId");

            migrationBuilder.CreateIndex(
                name: "IX_BayiMesajlar_UrunId",
                table: "BayiMesajlar",
                column: "UrunId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BayiMesajlar");
        }
    }
}
