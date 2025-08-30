using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DogRaces.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGlobalConfigurationsAndUpdateRace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RaceName",
                table: "Races",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<List<int>>(
                name: "RandomNumbers",
                table: "Races",
                type: "integer[]",
                nullable: false);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Races",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "global_configuration",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    min_ticket_stake = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 1.00m),
                    max_ticket_win = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 10000.00m),
                    min_number_of_active_rounds = table.Column<int>(type: "integer", nullable: false, defaultValue: 7)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_global_configuration", x => x.id);
                });

            migrationBuilder.InsertData(
                table: "global_configuration",
                columns: new[] { "id", "max_ticket_win", "min_number_of_active_rounds", "min_ticket_stake" },
                values: new object[] { 1, 10000.00m, 7, 1.00m });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "global_configuration");

            migrationBuilder.DropColumn(
                name: "RaceName",
                table: "Races");

            migrationBuilder.DropColumn(
                name: "RandomNumbers",
                table: "Races");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Races");
        }
    }
}
