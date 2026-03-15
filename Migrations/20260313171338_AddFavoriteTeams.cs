using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sportify.Migrations
{
    /// <inheritdoc />
    public partial class AddFavoriteTeams : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KullaniciTakimlar",
                columns: table => new
                {
                    KullaniciId = table.Column<int>(type: "int", nullable: false),
                    TakimId = table.Column<int>(type: "int", nullable: false),
                    EklenmeTarihi = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KullaniciTakimlar", x => new { x.KullaniciId, x.TakimId });
                    table.ForeignKey(
                        name: "FK_KullaniciTakimlar_Kullanicilar_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KullaniciTakimlar_Takimlar_TakimId",
                        column: x => x.TakimId,
                        principalTable: "Takimlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KullaniciTakimlar_TakimId",
                table: "KullaniciTakimlar",
                column: "TakimId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KullaniciTakimlar");
        }
    }
}
