using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DogRaces.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTableNamesToSnakeCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bets_Races_RaceId",
                table: "Bets");

            migrationBuilder.DropForeignKey(
                name: "FK_Bets_Tickets_TicketId",
                table: "Bets");

            migrationBuilder.DropForeignKey(
                name: "FK_RaceOdds_Races_RaceId",
                table: "RaceOdds");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tickets",
                table: "Tickets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Races",
                table: "Races");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Bets",
                table: "Bets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RaceOdds",
                table: "RaceOdds");

            migrationBuilder.RenameTable(
                name: "Tickets",
                newName: "tickets");

            migrationBuilder.RenameTable(
                name: "Races",
                newName: "races");

            migrationBuilder.RenameTable(
                name: "Bets",
                newName: "bets");

            migrationBuilder.RenameTable(
                name: "RaceOdds",
                newName: "race_odds");

            migrationBuilder.RenameIndex(
                name: "IX_Bets_TicketId",
                table: "bets",
                newName: "IX_bets_TicketId");

            migrationBuilder.RenameIndex(
                name: "IX_Bets_RaceId",
                table: "bets",
                newName: "IX_bets_RaceId");

            migrationBuilder.RenameIndex(
                name: "IX_RaceOdds_RaceId_Selection",
                table: "race_odds",
                newName: "IX_race_odds_RaceId_Selection");

            migrationBuilder.AddPrimaryKey(
                name: "PK_tickets",
                table: "tickets",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_races",
                table: "races",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_bets",
                table: "bets",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_race_odds",
                table: "race_odds",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_bets_races_RaceId",
                table: "bets",
                column: "RaceId",
                principalTable: "races",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_bets_tickets_TicketId",
                table: "bets",
                column: "TicketId",
                principalTable: "tickets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_race_odds_races_RaceId",
                table: "race_odds",
                column: "RaceId",
                principalTable: "races",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_bets_races_RaceId",
                table: "bets");

            migrationBuilder.DropForeignKey(
                name: "FK_bets_tickets_TicketId",
                table: "bets");

            migrationBuilder.DropForeignKey(
                name: "FK_race_odds_races_RaceId",
                table: "race_odds");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tickets",
                table: "tickets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_races",
                table: "races");

            migrationBuilder.DropPrimaryKey(
                name: "PK_bets",
                table: "bets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_race_odds",
                table: "race_odds");

            migrationBuilder.RenameTable(
                name: "tickets",
                newName: "Tickets");

            migrationBuilder.RenameTable(
                name: "races",
                newName: "Races");

            migrationBuilder.RenameTable(
                name: "bets",
                newName: "Bets");

            migrationBuilder.RenameTable(
                name: "race_odds",
                newName: "RaceOdds");

            migrationBuilder.RenameIndex(
                name: "IX_bets_TicketId",
                table: "Bets",
                newName: "IX_Bets_TicketId");

            migrationBuilder.RenameIndex(
                name: "IX_bets_RaceId",
                table: "Bets",
                newName: "IX_Bets_RaceId");

            migrationBuilder.RenameIndex(
                name: "IX_race_odds_RaceId_Selection",
                table: "RaceOdds",
                newName: "IX_RaceOdds_RaceId_Selection");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tickets",
                table: "Tickets",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Races",
                table: "Races",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Bets",
                table: "Bets",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RaceOdds",
                table: "RaceOdds",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Bets_Races_RaceId",
                table: "Bets",
                column: "RaceId",
                principalTable: "Races",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Bets_Tickets_TicketId",
                table: "Bets",
                column: "TicketId",
                principalTable: "Tickets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RaceOdds_Races_RaceId",
                table: "RaceOdds",
                column: "RaceId",
                principalTable: "Races",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
