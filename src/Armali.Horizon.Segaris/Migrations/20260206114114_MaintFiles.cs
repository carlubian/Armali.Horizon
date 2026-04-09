using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Armali.Horizon.Segaris.Migrations
{
    /// <inheritdoc />
    public partial class MaintFiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MaintCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MaintStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Color = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MaintEntities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Details = table.Column<string>(type: "TEXT", nullable: false),
                    CategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    StatusId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsPrivate = table.Column<bool>(type: "INTEGER", nullable: false),
                    Creator = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintEntities_MaintCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "MaintCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MaintEntities_MaintStatuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "MaintStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "MaintCategories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Platform" },
                    { 2, "Appliances" },
                    { 3, "Furniture" },
                    { 4, "Decoration" },
                    { 5, "Vehicles" },
                    { 6, "Computers" },
                    { 7, "Assets" }
                });

            migrationBuilder.InsertData(
                table: "MaintStatuses",
                columns: new[] { "Id", "Color", "Name" },
                values: new object[,]
                {
                    { 1, "blue", "Created" },
                    { 2, "gold", "Active" },
                    { 3, "green", "Completed" },
                    { 4, "red", "Canceled" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_MaintEntities_CategoryId",
                table: "MaintEntities",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintEntities_StatusId",
                table: "MaintEntities",
                column: "StatusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MaintEntities");

            migrationBuilder.DropTable(
                name: "MaintCategories");

            migrationBuilder.DropTable(
                name: "MaintStatuses");
        }
    }
}
