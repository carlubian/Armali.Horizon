using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Armali.Horizon.Segaris.Migrations
{
    /// <inheritdoc />
    public partial class AddTravelLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "TravelEntities",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "TravelEntities");
        }
    }
}
