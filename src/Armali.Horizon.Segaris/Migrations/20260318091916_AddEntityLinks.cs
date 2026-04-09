using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Armali.Horizon.Segaris.Migrations
{
    /// <inheritdoc />
    public partial class AddEntityLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "OpexEntities",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AssetId",
                table: "MaintEntities",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "CapexEntities",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "AssetEntities",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "OpexEntities");

            migrationBuilder.DropColumn(
                name: "AssetId",
                table: "MaintEntities");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "CapexEntities");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "AssetEntities");
        }
    }
}
