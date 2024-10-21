using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebCalenderAPI.Migrations
{
    /// <inheritdoc />
    public partial class addNewSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Schedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    fromX = table.Column<int>(type: "int", nullable: false),
                    fromY = table.Column<int>(type: "int", nullable: false),
                    toX = table.Column<int>(type: "int", nullable: false),
                    toY = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schedules", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Schedules");
        }
    }
}
