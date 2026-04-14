using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Armali.Horizon.Segaris.Migrations
{
    /// <inheritdoc />
    public partial class Admin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdminCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AdminEntities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    CategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsPrivate = table.Column<bool>(type: "INTEGER", nullable: false),
                    Creator = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdminEntities_AdminCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "AdminCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AdminSubEntities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DueDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsCompleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    ProcessId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminSubEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdminSubEntities_AdminEntities_ProcessId",
                        column: x => x.ProcessId,
                        principalTable: "AdminEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AdminCategories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Government" },
                    { 2, "Employment" },
                    { 3, "Banking" },
                    { 4, "Insurance" },
                    { 5, "Legal" },
                    { 6, "Housing" },
                    { 7, "Vehicles" },
                    { 8, "Other" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdminEntities_CategoryId",
                table: "AdminEntities",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AdminSubEntities_ProcessId",
                table: "AdminSubEntities",
                column: "ProcessId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminSubEntities");

            migrationBuilder.DropTable(
                name: "AdminEntities");

            migrationBuilder.DropTable(
                name: "AdminCategories");
        }
    }
}
