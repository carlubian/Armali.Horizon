using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Armali.Horizon.Segaris.Migrations
{
    /// <inheritdoc />
    public partial class FixMoodDuplicity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "MoodCategories",
                keyColumn: "Id",
                keyValue: 37,
                column: "Name",
                value: "  Self-Care");

            migrationBuilder.UpdateData(
                table: "MoodCategories",
                keyColumn: "Id",
                keyValue: 38,
                column: "Name",
                value: "  Healing");

            migrationBuilder.UpdateData(
                table: "MoodCategories",
                keyColumn: "Id",
                keyValue: 39,
                column: "Name",
                value: "  Indoor");

            migrationBuilder.UpdateData(
                table: "MoodCategories",
                keyColumn: "Id",
                keyValue: 40,
                column: "Name",
                value: "  Indifferent");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "MoodCategories",
                keyColumn: "Id",
                keyValue: 37,
                column: "Name",
                value: "  Relaxed");

            migrationBuilder.UpdateData(
                table: "MoodCategories",
                keyColumn: "Id",
                keyValue: 38,
                column: "Name",
                value: "  Satified");

            migrationBuilder.UpdateData(
                table: "MoodCategories",
                keyColumn: "Id",
                keyValue: 39,
                column: "Name",
                value: "  Relieved");

            migrationBuilder.UpdateData(
                table: "MoodCategories",
                keyColumn: "Id",
                keyValue: 40,
                column: "Name",
                value: "  Serene");
        }
    }
}
