using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Armali.Horizon.Segaris.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFirebirdStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "FirebirdStatuses",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Available");

            migrationBuilder.UpdateData(
                table: "FirebirdStatuses",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Unavailable");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "FirebirdStatuses",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Active");

            migrationBuilder.UpdateData(
                table: "FirebirdStatuses",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Inactive");
        }
    }
}
