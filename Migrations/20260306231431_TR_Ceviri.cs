using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sportify.Migrations
{
    /// <inheritdoc />
    public partial class TR_Ceviri : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Matches");

            migrationBuilder.DropTable(
                name: "Players");

            migrationBuilder.DropTable(
                name: "Standings");

            migrationBuilder.DropTable(
                name: "Teams");

            migrationBuilder.DropTable(
                name: "Leagues");

            migrationBuilder.CreateTable(
                name: "Ligler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Logo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ulke = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Bayrak = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sezon = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ligler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Maclar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EvSahibi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Deplasman = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EvSahibiLogo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeplasmanLogo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EvSahibiSkor = table.Column<int>(type: "int", nullable: true),
                    DeplasmanSkor = table.Column<int>(type: "int", nullable: true),
                    Durum = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LigAdi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LigLogo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tarih = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Dakika = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Maclar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Oyuncular",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Foto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Uyruk = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Yas = table.Column<int>(type: "int", nullable: false),
                    Mevki = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TakimAdi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TakimLogo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Istatistikler_MacSayisi = table.Column<int>(type: "int", nullable: true),
                    Istatistikler_Goller = table.Column<int>(type: "int", nullable: true),
                    Istatistikler_Asistler = table.Column<int>(type: "int", nullable: true),
                    Istatistikler_SariKartlar = table.Column<int>(type: "int", nullable: true),
                    Istatistikler_KirmiziKartlar = table.Column<int>(type: "int", nullable: true),
                    Istatistikler_Reyting = table.Column<double>(type: "float", nullable: true),
                    Istatistikler_OynananDakika = table.Column<int>(type: "int", nullable: true),
                    Istatistikler_Sutlar = table.Column<int>(type: "int", nullable: true),
                    Istatistikler_IsabetliSutlar = table.Column<int>(type: "int", nullable: true),
                    Istatistikler_Paslar = table.Column<int>(type: "int", nullable: true),
                    Istatistikler_PasIsabeti = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Oyuncular", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Takimlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Logo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ulke = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KurulusYili = table.Column<int>(type: "int", nullable: false),
                    Stadyum = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Istatistikler_Oynanan = table.Column<int>(type: "int", nullable: true),
                    Istatistikler_Galibiyet = table.Column<int>(type: "int", nullable: true),
                    Istatistikler_Beraberlik = table.Column<int>(type: "int", nullable: true),
                    Istatistikler_Maglubiyet = table.Column<int>(type: "int", nullable: true),
                    Istatistikler_AtilanGol = table.Column<int>(type: "int", nullable: true),
                    Istatistikler_YenilenGol = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Takimlar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PuanDurumlari",
                columns: table => new
                {
                    TakimId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Sira = table.Column<int>(type: "int", nullable: false),
                    TakimAdi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TakimLogo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Oynanan = table.Column<int>(type: "int", nullable: false),
                    Galibiyet = table.Column<int>(type: "int", nullable: false),
                    Beraberlik = table.Column<int>(type: "int", nullable: false),
                    Maglubiyet = table.Column<int>(type: "int", nullable: false),
                    AtilanGol = table.Column<int>(type: "int", nullable: false),
                    YenilenGol = table.Column<int>(type: "int", nullable: false),
                    GolFarki = table.Column<int>(type: "int", nullable: false),
                    Puan = table.Column<int>(type: "int", nullable: false),
                    Form = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LigId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PuanDurumlari", x => x.TakimId);
                    table.ForeignKey(
                        name: "FK_PuanDurumlari_Ligler_LigId",
                        column: x => x.LigId,
                        principalTable: "Ligler",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PuanDurumlari_LigId",
                table: "PuanDurumlari",
                column: "LigId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Maclar");

            migrationBuilder.DropTable(
                name: "Oyuncular");

            migrationBuilder.DropTable(
                name: "PuanDurumlari");

            migrationBuilder.DropTable(
                name: "Takimlar");

            migrationBuilder.DropTable(
                name: "Ligler");

            migrationBuilder.CreateTable(
                name: "Leagues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Flag = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Logo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Season = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leagues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Matches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AwayScore = table.Column<int>(type: "int", nullable: true),
                    AwayTeam = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AwayTeamLogo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HomeScore = table.Column<int>(type: "int", nullable: true),
                    HomeTeam = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HomeTeamLogo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LeagueLogo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LeagueName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Minute = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Age = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nationality = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Photo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Position = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TeamLogo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TeamName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Stats_Appearances = table.Column<int>(type: "int", nullable: true),
                    Stats_Assists = table.Column<int>(type: "int", nullable: true),
                    Stats_Goals = table.Column<int>(type: "int", nullable: true),
                    Stats_MinutesPlayed = table.Column<int>(type: "int", nullable: true),
                    Stats_PassAccuracy = table.Column<double>(type: "float", nullable: true),
                    Stats_Passes = table.Column<int>(type: "int", nullable: true),
                    Stats_Rating = table.Column<double>(type: "float", nullable: true),
                    Stats_RedCards = table.Column<int>(type: "int", nullable: true),
                    Stats_Shots = table.Column<int>(type: "int", nullable: true),
                    Stats_ShotsOnTarget = table.Column<int>(type: "int", nullable: true),
                    Stats_YellowCards = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Founded = table.Column<int>(type: "int", nullable: false),
                    Logo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Stadium = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Stats_Drawn = table.Column<int>(type: "int", nullable: true),
                    Stats_GoalsAgainst = table.Column<int>(type: "int", nullable: true),
                    Stats_GoalsFor = table.Column<int>(type: "int", nullable: true),
                    Stats_Lost = table.Column<int>(type: "int", nullable: true),
                    Stats_Played = table.Column<int>(type: "int", nullable: true),
                    Stats_Won = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Standings",
                columns: table => new
                {
                    TeamId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Drawn = table.Column<int>(type: "int", nullable: false),
                    Form = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GoalDiff = table.Column<int>(type: "int", nullable: false),
                    GoalsAgainst = table.Column<int>(type: "int", nullable: false),
                    GoalsFor = table.Column<int>(type: "int", nullable: false),
                    LeagueId = table.Column<int>(type: "int", nullable: true),
                    Lost = table.Column<int>(type: "int", nullable: false),
                    Played = table.Column<int>(type: "int", nullable: false),
                    Points = table.Column<int>(type: "int", nullable: false),
                    Rank = table.Column<int>(type: "int", nullable: false),
                    TeamLogo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TeamName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Won = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Standings", x => x.TeamId);
                    table.ForeignKey(
                        name: "FK_Standings_Leagues_LeagueId",
                        column: x => x.LeagueId,
                        principalTable: "Leagues",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Standings_LeagueId",
                table: "Standings",
                column: "LeagueId");
        }
    }
}
