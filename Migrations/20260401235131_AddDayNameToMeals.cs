using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DietitianApp.Migrations
{
    /// <inheritdoc />
    public partial class AddDayNameToMeals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DayName",
                table: "DietMeals",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DayName",
                table: "DietMeals");
        }
    }
}
