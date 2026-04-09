using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Armali.Horizon.Segaris.Migrations
{
    /// <inheritdoc />
    public partial class InvOrderEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InvOrderStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Color = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvOrderStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InvOrderEntities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PurchaseDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ReceptionDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StatusId = table.Column<int>(type: "INTEGER", nullable: false),
                    VendorId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsPrivate = table.Column<bool>(type: "INTEGER", nullable: false),
                    Creator = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvOrderEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvOrderEntities_InvOrderStatuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "InvOrderStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvOrderEntities_InvVendorEntities_VendorId",
                        column: x => x.VendorId,
                        principalTable: "InvVendorEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InvOrderSubEntities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    ItemCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Amount = table.Column<double>(type: "REAL", nullable: false),
                    OrderId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvOrderSubEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvOrderSubEntities_InvItemEntities_ItemId",
                        column: x => x.ItemId,
                        principalTable: "InvItemEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvOrderSubEntities_InvOrderEntities_OrderId",
                        column: x => x.OrderId,
                        principalTable: "InvOrderEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "InvOrderStatuses",
                columns: new[] { "Id", "Color", "Name" },
                values: new object[,]
                {
                    { 1, "blue", "Planning" },
                    { 2, "gold", "In Progress" },
                    { 3, "green", "Completed" },
                    { 4, "red", "Canceled" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_InvOrderEntities_StatusId",
                table: "InvOrderEntities",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_InvOrderEntities_VendorId",
                table: "InvOrderEntities",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_InvOrderSubEntities_ItemId",
                table: "InvOrderSubEntities",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_InvOrderSubEntities_OrderId",
                table: "InvOrderSubEntities",
                column: "OrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvOrderSubEntities");

            migrationBuilder.DropTable(
                name: "InvOrderEntities");

            migrationBuilder.DropTable(
                name: "InvOrderStatuses");
        }
    }
}
