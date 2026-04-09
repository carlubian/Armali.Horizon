using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Armali.Horizon.Segaris.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CapexCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CapexCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CapexStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Color = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CapexStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CapexEntities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Amount = table.Column<double>(type: "REAL", nullable: false),
                    CategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    StatusId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsPrivate = table.Column<bool>(type: "INTEGER", nullable: false),
                    Creator = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CapexEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CapexEntities_CapexCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "CapexCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CapexEntities_CapexStatuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "CapexStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "CapexCategories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Government" },
                    { 2, "Employment" },
                    { 3, "Investment" },
                    { 4, "Food and Drinks" },
                    { 5, "Entertainment" },
                    { 6, "Education" },
                    { 7, "Social Expenses" },
                    { 8, "Medicines" },
                    { 9, "Assets" },
                    { 10, "Public Transport" },
                    { 11, "Housing" }
                });

            migrationBuilder.InsertData(
                table: "CapexStatuses",
                columns: new[] { "Id", "Color", "Name" },
                values: new object[,]
                {
                    { 1, "blue", "Planning" },
                    { 2, "green", "Completed" },
                    { 3, "red", "Canceled" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CapexEntities_CategoryId",
                table: "CapexEntities",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CapexEntities_StatusId",
                table: "CapexEntities",
                column: "StatusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CapexEntities");

            migrationBuilder.DropTable(
                name: "CapexCategories");

            migrationBuilder.DropTable(
                name: "CapexStatuses");
        }
    }
}
