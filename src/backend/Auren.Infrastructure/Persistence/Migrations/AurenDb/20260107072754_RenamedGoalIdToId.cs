using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auren.API.Migrations.AurenDb
{
    /// <inheritdoc />
    public partial class RenamedGoalIdToId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "GoalId",
                table: "Goals",
                newName: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Goals",
                newName: "GoalId");
        }
    }
}
