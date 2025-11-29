using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auren.API.Migrations.AurenDb
{
    /// <inheritdoc />
    public partial class UpdatedGoal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Spent",
                table: "Goals",
                type: "decimal(12,2)",
                precision: 12,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(12,2)",
                oldPrecision: 12,
                oldScale: 2);

            migrationBuilder.AddColumn<int>(
                name: "CompletionPercentage",
                table: "Goals",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TimeRemaining",
                table: "Goals",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletionPercentage",
                table: "Goals");

            migrationBuilder.DropColumn(
                name: "TimeRemaining",
                table: "Goals");

            migrationBuilder.AlterColumn<decimal>(
                name: "Spent",
                table: "Goals",
                type: "decimal(12,2)",
                precision: 12,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(12,2)",
                oldPrecision: 12,
                oldScale: 2,
                oldNullable: true);
        }
    }
}
