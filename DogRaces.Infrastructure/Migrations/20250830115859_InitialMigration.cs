using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System;

#nullable disable

namespace DogRaces.Infrastructure.Migrations;

/// <inheritdoc />
public partial class InitialMigration : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Races",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                StartTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                EndTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                Result = table.Column<string>(type: "text", nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                ResultDeterminedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                ResultPublishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Races", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Tickets",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Status = table.Column<string>(type: "text", nullable: false),
                TotalStake = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                TotalPayout = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Tickets", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "RaceOdds",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                RaceId = table.Column<long>(type: "bigint", nullable: false),
                Selection = table.Column<int>(type: "integer", nullable: false),
                Odds = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RaceOdds", x => x.Id);
                table.ForeignKey(
                    name: "FK_RaceOdds_Races_RaceId",
                    column: x => x.RaceId,
                    principalTable: "Races",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Bets",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                RaceId = table.Column<long>(type: "bigint", nullable: false),
                TicketId = table.Column<Guid>(type: "uuid", nullable: false),
                Selection = table.Column<int>(type: "integer", nullable: false),
                BetType = table.Column<string>(type: "text", nullable: false),
                Odds = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                IsWinning = table.Column<bool>(type: "boolean", nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Bets", x => x.Id);
                table.ForeignKey(
                    name: "FK_Bets_Races_RaceId",
                    column: x => x.RaceId,
                    principalTable: "Races",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_Bets_Tickets_TicketId",
                    column: x => x.TicketId,
                    principalTable: "Tickets",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Bets_RaceId",
            table: "Bets",
            column: "RaceId");

        migrationBuilder.CreateIndex(
            name: "IX_Bets_TicketId",
            table: "Bets",
            column: "TicketId");

        migrationBuilder.CreateIndex(
            name: "IX_RaceOdds_RaceId_Selection",
            table: "RaceOdds",
            columns: new[] { "RaceId", "Selection" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Bets");

        migrationBuilder.DropTable(
            name: "RaceOdds");

        migrationBuilder.DropTable(
            name: "Tickets");

        migrationBuilder.DropTable(
            name: "Races");
    }
}