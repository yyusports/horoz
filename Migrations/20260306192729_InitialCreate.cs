using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sportify.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Leagues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Logo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Flag = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                    HomeTeam = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AwayTeam = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HomeTeamLogo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AwayTeamLogo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HomeScore = table.Column<int>(type: "int", nullable: true),
                    AwayScore = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LeagueName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LeagueLogo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Minute = table.Column<int>(type: "int", nullable: true)
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
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Photo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nationality = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Age = table.Column<int>(type: "int", nullable: false),
                    Position = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TeamName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TeamLogo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Stats_Appearances = table.Column<int>(type: "int", nullable: true),
                    Stats_Goals = table.Column<int>(type: "int", nullable: true),
                    Stats_Assists = table.Column<int>(type: "int", nullable: true),
                    Stats_YellowCards = table.Column<int>(type: "int", nullable: true),
                    Stats_RedCards = table.Column<int>(type: "int", nullable: true),
                    Stats_Rating = table.Column<double>(type: "float", nullable: true),
                    Stats_MinutesPlayed = table.Column<int>(type: "int", nullable: true),
                    Stats_Shots = table.Column<int>(type: "int", nullable: true),
                    Stats_ShotsOnTarget = table.Column<int>(type: "int", nullable: true),
                    Stats_Passes = table.Column<int>(type: "int", nullable: true),
                    Stats_PassAccuracy = table.Column<double>(type: "float", nullable: true)
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
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Logo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Founded = table.Column<int>(type: "int", nullable: false),
                    Stadium = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Stats_Played = table.Column<int>(type: "int", nullable: true),
                    Stats_Won = table.Column<int>(type: "int", nullable: true),
                    Stats_Drawn = table.Column<int>(type: "int", nullable: true),
                    Stats_Lost = table.Column<int>(type: "int", nullable: true),
                    Stats_GoalsFor = table.Column<int>(type: "int", nullable: true),
                    Stats_GoalsAgainst = table.Column<int>(type: "int", nullable: true)
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
                    Rank = table.Column<int>(type: "int", nullable: false),
                    TeamName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TeamLogo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Played = table.Column<int>(type: "int", nullable: false),
                    Won = table.Column<int>(type: "int", nullable: false),
                    Drawn = table.Column<int>(type: "int", nullable: false),
                    Lost = table.Column<int>(type: "int", nullable: false),
                    GoalsFor = table.Column<int>(type: "int", nullable: false),
                    GoalsAgainst = table.Column<int>(type: "int", nullable: false),
                    GoalDiff = table.Column<int>(type: "int", nullable: false),
                    Points = table.Column<int>(type: "int", nullable: false),
                    Form = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LeagueId = table.Column<int>(type: "int", nullable: true)
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
        }
    }
}
