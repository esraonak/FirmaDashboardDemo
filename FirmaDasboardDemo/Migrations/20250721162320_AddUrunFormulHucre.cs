using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FirmaDasboardDemo.Migrations
{
    /// <inheritdoc />
    public partial class AddUrunFormulHucre : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Urun",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirmaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Urun", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Urun_Firmalar_FirmaId",
                        column: x => x.FirmaId,
                        principalTable: "Firmalar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FormulTablosu",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UrunId = table.Column<int>(type: "int", nullable: false),
                    CalisanId = table.Column<int>(type: "int", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormulTablosu", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormulTablosu_FirmaCalisanlari_CalisanId",
                        column: x => x.CalisanId,
                        principalTable: "FirmaCalisanlari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FormulTablosu_Urun_UrunId",
                        column: x => x.UrunId,
                        principalTable: "Urun",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Hucre",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HucreAdi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Formul = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsFormul = table.Column<bool>(type: "bit", nullable: false),
                    TabloId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hucre", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Hucre_FormulTablosu_TabloId",
                        column: x => x.TabloId,
                        principalTable: "FormulTablosu",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FormulTablosu_CalisanId",
                table: "FormulTablosu",
                column: "CalisanId");

            migrationBuilder.CreateIndex(
                name: "IX_FormulTablosu_UrunId",
                table: "FormulTablosu",
                column: "UrunId");

            migrationBuilder.CreateIndex(
                name: "IX_Hucre_TabloId",
                table: "Hucre",
                column: "TabloId");

            migrationBuilder.CreateIndex(
                name: "IX_Urun_FirmaId",
                table: "Urun",
                column: "FirmaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Hucre");

            migrationBuilder.DropTable(
                name: "FormulTablosu");

            migrationBuilder.DropTable(
                name: "Urun");
        }
    }
}
