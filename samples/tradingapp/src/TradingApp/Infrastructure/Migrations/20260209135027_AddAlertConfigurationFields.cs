using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TradingApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAlertConfigurationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ComparisonType",
                table: "Alerts",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Threshold",
                table: "Alerts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Timeframe",
                table: "Alerts",
                type: "TEXT",
                maxLength: 16,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ComparisonType",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "Threshold",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "Timeframe",
                table: "Alerts");
        }
    }
}
