using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DogRaces.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ImplementBetTypeOddsScaling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_race_odds_RaceId_Selection",
                table: "race_odds");

            migrationBuilder.AddColumn<int>(
                name: "BetType",
                table: "race_odds",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_race_odds_RaceId_Selection_BetType",
                table: "race_odds",
                columns: new[] { "RaceId", "Selection", "BetType" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_race_odds_RaceId_Selection_BetType",
                table: "race_odds");

            migrationBuilder.DropColumn(
                name: "BetType",
                table: "race_odds");

            migrationBuilder.CreateIndex(
                name: "IX_race_odds_RaceId_Selection",
                table: "race_odds",
                columns: new[] { "RaceId", "Selection" },
                unique: true);
        }
    }
}
