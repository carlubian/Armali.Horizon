using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Armali.Horizon.Segaris.Migrations
{
    /// <inheritdoc />
    public partial class AddInvOrderSubEntityProjectLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "InvOrderSubEntities",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "InvOrderSubEntities");
        }
    }
}
