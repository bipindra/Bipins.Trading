using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TradingApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAlertOrderExecutionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EnableAutoExecute",
                table: "Alerts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "OrderLimitPrice",
                table: "Alerts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OrderQuantity",
                table: "Alerts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrderSideOverride",
                table: "Alerts",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OrderStopPrice",
                table: "Alerts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrderTimeInForce",
                table: "Alerts",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrderType",
                table: "Alerts",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnableAutoExecute",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "OrderLimitPrice",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "OrderQuantity",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "OrderSideOverride",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "OrderStopPrice",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "OrderTimeInForce",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "OrderType",
                table: "Alerts");
        }
    }
}
