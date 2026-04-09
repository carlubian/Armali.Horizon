using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Armali.Horizon.Segaris.Migrations
{
    /// <inheritdoc />
    public partial class FirebirdEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FirebirdCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FirebirdCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FirebirdStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Color = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FirebirdStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FirebirdEntities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    CategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    StatusId = table.Column<int>(type: "INTEGER", nullable: false),
                    Location = table.Column<string>(type: "TEXT", nullable: false),
                    Birthday = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsAware = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsPrivate = table.Column<bool>(type: "INTEGER", nullable: false),
                    Creator = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FirebirdEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FirebirdEntities_FirebirdCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "FirebirdCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FirebirdEntities_FirebirdStatuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "FirebirdStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FirebirdSubEntities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    FirebirdId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FirebirdSubEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FirebirdSubEntities_FirebirdEntities_FirebirdId",
                        column: x => x.FirebirdId,
                        principalTable: "FirebirdEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "FirebirdCategories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Cat A" },
                    { 2, "Cat B" },
                    { 3, "Cat C" },
                    { 4, "Cat D" },
                    { 5, "Cat E" },
                    { 6, "Cat F" }
                });

            migrationBuilder.InsertData(
                table: "FirebirdStatuses",
                columns: new[] { "Id", "Color", "Name" },
                values: new object[,]
                {
                    { 1, "blue", "Unknown" },
                    { 2, "green", "Active" },
                    { 3, "red", "Inactive" },
                    { 4, "gray", "Blocked" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_FirebirdEntities_CategoryId",
                table: "FirebirdEntities",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_FirebirdEntities_StatusId",
                table: "FirebirdEntities",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_FirebirdSubEntities_FirebirdId",
                table: "FirebirdSubEntities",
                column: "FirebirdId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FirebirdSubEntities");

            migrationBuilder.DropTable(
                name: "FirebirdEntities");

            migrationBuilder.DropTable(
                name: "FirebirdCategories");

            migrationBuilder.DropTable(
                name: "FirebirdStatuses");
        }
    }
}
