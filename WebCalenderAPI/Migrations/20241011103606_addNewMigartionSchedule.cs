using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebCalenderAPI.Migrations
{
    /// <inheritdoc />
    public partial class addNewMigartionSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "reason",
                table: "Schedules",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "reason",
                table: "Schedules");
        }
    }
}
