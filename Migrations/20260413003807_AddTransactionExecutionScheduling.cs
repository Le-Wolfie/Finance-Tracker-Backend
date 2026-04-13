using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinancialTracker.API.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionExecutionScheduling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BalanceAppliedAt",
                table: "Transactions",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExecutionMode",
                table: "Transactions",
                type: "varchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "ApplyImmediately")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "IsBalanceApplied",
                table: "Transactions",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BalanceAppliedAt",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "ExecutionMode",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "IsBalanceApplied",
                table: "Transactions");
        }
    }
}
