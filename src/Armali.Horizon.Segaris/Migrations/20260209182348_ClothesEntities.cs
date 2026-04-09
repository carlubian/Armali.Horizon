using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Armali.Horizon.Segaris.Migrations
{
    /// <inheritdoc />
    public partial class ClothesEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClothesCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClothesCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClothesStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Color = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClothesStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClothesWashTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClothesWashTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClothesEntities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GarmentCode = table.Column<string>(type: "TEXT", nullable: false),
                    CategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    StatusId = table.Column<int>(type: "INTEGER", nullable: false),
                    WashTypeId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsPrivate = table.Column<bool>(type: "INTEGER", nullable: false),
                    Creator = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClothesEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClothesEntities_ClothesCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ClothesCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClothesEntities_ClothesStatuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "ClothesStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClothesEntities_ClothesWashTypes_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ClothesWashTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ClothesCategories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Short T-Shirt" },
                    { 2, "Long T-Shirt" },
                    { 3, "Short Shirt" },
                    { 4, "Long Shirt" },
                    { 5, "Short Polo" },
                    { 6, "Long Polo" },
                    { 7, "Short Trouser" },
                    { 8, "Long Trouser" },
                    { 9, "Jacket" },
                    { 10, "Coat" },
                    { 11, "Footwear" }
                });

            migrationBuilder.InsertData(
                table: "ClothesStatuses",
                columns: new[] { "Id", "Color", "Name" },
                values: new object[,]
                {
                    { 1, "blue", "Planning" },
                    { 2, "green", "Active" },
                    { 3, "red", "Retired" }
                });

            migrationBuilder.InsertData(
                table: "ClothesWashTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "White Wash" },
                    { 2, "Color Wash" },
                    { 3, "Special Wash" },
                    { 4, "Wash Alone" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClothesEntities_CategoryId",
                table: "ClothesEntities",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ClothesEntities_StatusId",
                table: "ClothesEntities",
                column: "StatusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClothesEntities");

            migrationBuilder.DropTable(
                name: "ClothesCategories");

            migrationBuilder.DropTable(
                name: "ClothesStatuses");

            migrationBuilder.DropTable(
                name: "ClothesWashTypes");
        }
    }
}
