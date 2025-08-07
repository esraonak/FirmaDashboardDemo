using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FirmaDasboardDemo.Migrations
{
    /// <inheritdoc />
    public partial class MakeUrunIdNullableInBayiMesaj : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BayiMesajlar_Urun_UrunId",
                table: "BayiMesajlar");

            migrationBuilder.AlterColumn<int>(
                name: "UrunId",
                table: "BayiMesajlar",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_BayiMesajlar_Urun_UrunId",
                table: "BayiMesajlar",
                column: "UrunId",
                principalTable: "Urun",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BayiMesajlar_Urun_UrunId",
                table: "BayiMesajlar");

            migrationBuilder.AlterColumn<int>(
                name: "UrunId",
                table: "BayiMesajlar",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BayiMesajlar_Urun_UrunId",
                table: "BayiMesajlar",
                column: "UrunId",
                principalTable: "Urun",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
