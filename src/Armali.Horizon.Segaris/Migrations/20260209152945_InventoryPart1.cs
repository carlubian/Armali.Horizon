using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Armali.Horizon.Segaris.Migrations
{
    /// <inheritdoc />
    public partial class InventoryPart1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InvItemCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvItemCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InvItemStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Color = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvItemStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InvVendorStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Color = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvVendorStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InvVendorEntities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    StatusId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsPrivate = table.Column<bool>(type: "INTEGER", nullable: false),
                    Creator = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvVendorEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvVendorEntities_InvVendorStatuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "InvVendorStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InvItemEntities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    CurrentStock = table.Column<int>(type: "INTEGER", nullable: false),
                    MinStock = table.Column<int>(type: "INTEGER", nullable: false),
                    CategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    StatusId = table.Column<int>(type: "INTEGER", nullable: false),
                    VendorId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsPrivate = table.Column<bool>(type: "INTEGER", nullable: false),
                    Creator = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvItemEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvItemEntities_InvItemCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "InvItemCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvItemEntities_InvItemStatuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "InvItemStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvItemEntities_InvVendorEntities_VendorId",
                        column: x => x.VendorId,
                        principalTable: "InvVendorEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "InvItemCategories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Bath Amenities" },
                    { 2, "Beauty Products" },
                    { 3, "Cleaning Products" },
                    { 4, "Medicines" },
                    { 5, "Complements" },
                    { 6, "Raw Ingredients" },
                    { 7, "Food Condiments" },
                    { 8, "Drinks" },
                    { 9, "Meal Ready to Eat" },
                    { 10, "Office Supplies" },
                    { 11, "Other Consumibles" }
                });

            migrationBuilder.InsertData(
                table: "InvItemStatuses",
                columns: new[] { "Id", "Color", "Name" },
                values: new object[,]
                {
                    { 1, "green", "Active" },
                    { 2, "red", "Deprecated" },
                    { 3, "blue", "Replaced" }
                });

            migrationBuilder.InsertData(
                table: "InvVendorStatuses",
                columns: new[] { "Id", "Color", "Name" },
                values: new object[,]
                {
                    { 1, "blue", "Planning" },
                    { 2, "green", "Active" },
                    { 3, "red", "Closed" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_InvItemEntities_CategoryId",
                table: "InvItemEntities",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_InvItemEntities_StatusId",
                table: "InvItemEntities",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_InvItemEntities_VendorId",
                table: "InvItemEntities",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_InvVendorEntities_StatusId",
                table: "InvVendorEntities",
                column: "StatusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvItemEntities");

            migrationBuilder.DropTable(
                name: "InvItemCategories");

            migrationBuilder.DropTable(
                name: "InvItemStatuses");

            migrationBuilder.DropTable(
                name: "InvVendorEntities");

            migrationBuilder.DropTable(
                name: "InvVendorStatuses");
        }
    }
}
