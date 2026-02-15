using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auren.API.Migrations.AurenDb
{
    /// <inheritdoc />
    public partial class AddEmojiToGoal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Emoji",
                table: "Goals",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Emoji",
                table: "Goals");
        }
    }
}
