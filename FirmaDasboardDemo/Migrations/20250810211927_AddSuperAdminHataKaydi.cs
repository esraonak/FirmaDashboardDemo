using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FirmaDasboardDemo.Migrations
{
    /// <inheritdoc />
    public partial class AddSuperAdminHataKaydi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SuperAdminHataKayitlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KullaniciRol = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KullaniciAdi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FirmaSeo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tarih = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HataMesaji = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StackTrace = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuperAdminHataKayitlari", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SuperAdminHataKayitlari");
        }
    }
}
