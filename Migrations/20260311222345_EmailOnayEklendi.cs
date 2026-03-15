using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sportify.Migrations
{
    /// <inheritdoc />
    public partial class EmailOnayEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailOnayKodu",
                table: "Kullanicilar",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEmailOnayli",
                table: "Kullanicilar",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailOnayKodu",
                table: "Kullanicilar");

            migrationBuilder.DropColumn(
                name: "IsEmailOnayli",
                table: "Kullanicilar");
        }
    }
}
