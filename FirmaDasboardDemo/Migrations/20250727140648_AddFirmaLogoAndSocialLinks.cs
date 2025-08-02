using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FirmaDasboardDemo.Migrations
{
    /// <inheritdoc />
    public partial class AddFirmaLogoAndSocialLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FacebookUrl",
                table: "Firmalar",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "InstagramUrl",
                table: "Firmalar",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "Firmalar",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TwitterUrl",
                table: "Firmalar",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WebSitesi",
                table: "Firmalar",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FacebookUrl",
                table: "Firmalar");

            migrationBuilder.DropColumn(
                name: "InstagramUrl",
                table: "Firmalar");

            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "Firmalar");

            migrationBuilder.DropColumn(
                name: "TwitterUrl",
                table: "Firmalar");

            migrationBuilder.DropColumn(
                name: "WebSitesi",
                table: "Firmalar");
        }
    }
}
