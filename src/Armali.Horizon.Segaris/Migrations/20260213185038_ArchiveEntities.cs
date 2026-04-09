using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Armali.Horizon.Segaris.Migrations
{
    /// <inheritdoc />
    public partial class ArchiveEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ArchiveCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchiveCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ArchiveStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Color = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchiveStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ArchiveEntities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    File = table.Column<string>(type: "TEXT", nullable: false),
                    CategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    StatusId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsPrivate = table.Column<bool>(type: "INTEGER", nullable: false),
                    Creator = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchiveEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchiveEntities_ArchiveCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ArchiveCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArchiveEntities_ArchiveStatuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "ArchiveStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ArchiveCategories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Government" },
                    { 2, "Employment" },
                    { 3, "Banking" },
                    { 4, "Other Business" },
                    { 5, "Entertainment" },
                    { 6, "Social Affairs" },
                    { 7, "Medical" },
                    { 8, "Assets" }
                });

            migrationBuilder.InsertData(
                table: "ArchiveStatuses",
                columns: new[] { "Id", "Color", "Name" },
                values: new object[,]
                {
                    { 1, "gold", "Pending" },
                    { 2, "green", "Active" },
                    { 3, "red", "Deprecated" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveEntities_CategoryId",
                table: "ArchiveEntities",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveEntities_StatusId",
                table: "ArchiveEntities",
                column: "StatusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArchiveEntities");

            migrationBuilder.DropTable(
                name: "ArchiveCategories");

            migrationBuilder.DropTable(
                name: "ArchiveStatuses");
        }
    }
}
