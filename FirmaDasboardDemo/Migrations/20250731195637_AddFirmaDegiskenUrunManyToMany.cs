using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FirmaDasboardDemo.Migrations
{
    /// <inheritdoc />
    public partial class AddFirmaDegiskenUrunManyToMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UrunId",
                table: "FirmaDegiskenler");

            migrationBuilder.AddColumn<float>(
                name: "Deger",
                table: "FirmaDegiskenler",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.CreateTable(
                name: "FirmaDegiskenUrunler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirmaDegiskenId = table.Column<int>(type: "int", nullable: false),
                    UrunId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FirmaDegiskenUrunler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FirmaDegiskenUrunler_FirmaDegiskenler_FirmaDegiskenId",
                        column: x => x.FirmaDegiskenId,
                        principalTable: "FirmaDegiskenler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FirmaDegiskenUrunler_Urun_UrunId",
                        column: x => x.UrunId,
                        principalTable: "Urun",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FirmaDegiskenUrunler_FirmaDegiskenId",
                table: "FirmaDegiskenUrunler",
                column: "FirmaDegiskenId");

            migrationBuilder.CreateIndex(
                name: "IX_FirmaDegiskenUrunler_UrunId",
                table: "FirmaDegiskenUrunler",
                column: "UrunId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FirmaDegiskenUrunler");

            migrationBuilder.DropColumn(
                name: "Deger",
                table: "FirmaDegiskenler");

            migrationBuilder.AddColumn<int>(
                name: "UrunId",
                table: "FirmaDegiskenler",
                type: "int",
                nullable: true);
        }
    }
}
