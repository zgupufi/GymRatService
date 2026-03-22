using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymRatService.Migrations
{
    /// <inheritdoc />
    public partial class PersonalizedSplitsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserWeeklySplits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    userId = table.Column<string>(type: "text", nullable: false),
                    MondayWorkoutId = table.Column<string>(type: "text", nullable: true),
                    TuesdayWorkoutId = table.Column<string>(type: "text", nullable: true),
                    WednesdayWorkoutId = table.Column<string>(type: "text", nullable: true),
                    ThursdayWorkoutId = table.Column<string>(type: "text", nullable: true),
                    FridayWorkoutId = table.Column<string>(type: "text", nullable: true),
                    SaturdayWorkoutId = table.Column<string>(type: "text", nullable: true),
                    SundayWorkoutId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserWeeklySplits", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserWeeklySplits");
        }
    }
}
