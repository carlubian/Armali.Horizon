using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Armali.Horizon.Segaris.Migrations
{
    /// <inheritdoc />
    public partial class TravelFiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TravelCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TravelCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TravelCostCenters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TravelCostCenters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TravelStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Color = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TravelStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TravelSubEntityCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TravelSubEntityCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TravelEntities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    CategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    StatusId = table.Column<int>(type: "INTEGER", nullable: false),
                    CostCenterId = table.Column<int>(type: "INTEGER", nullable: false),
                    Destination = table.Column<string>(type: "TEXT", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Pax = table.Column<int>(type: "INTEGER", nullable: false),
                    IsPrivate = table.Column<bool>(type: "INTEGER", nullable: false),
                    Creator = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TravelEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TravelEntities_TravelCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "TravelCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TravelEntities_TravelCostCenters_CostCenterId",
                        column: x => x.CostCenterId,
                        principalTable: "TravelCostCenters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TravelEntities_TravelStatuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "TravelStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TravelSubEntities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    CategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Amount = table.Column<double>(type: "REAL", nullable: false),
                    TravelId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TravelSubEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TravelSubEntities_TravelEntities_TravelId",
                        column: x => x.TravelId,
                        principalTable: "TravelEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TravelSubEntities_TravelSubEntityCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "TravelSubEntityCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "TravelCategories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Local" },
                    { 2, "Regional" },
                    { 3, "National" },
                    { 4, "Schengen" },
                    { 5, "Non-Schengen" }
                });

            migrationBuilder.InsertData(
                table: "TravelCostCenters",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Armali" },
                    { 2, "Common Fund" },
                    { 3, "AMI3" },
                    { 4, "Other CC" }
                });

            migrationBuilder.InsertData(
                table: "TravelStatuses",
                columns: new[] { "Id", "Color", "Name" },
                values: new object[,]
                {
                    { 1, "blue", "Planning" },
                    { 2, "gold", "Active" },
                    { 3, "green", "Completed" },
                    { 4, "red", "Canceled" }
                });

            migrationBuilder.InsertData(
                table: "TravelSubEntityCategories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Hotels" },
                    { 2, "Other Lodging" },
                    { 3, "Airplane" },
                    { 4, "Train" },
                    { 5, "Car Rental" },
                    { 6, "Metro" },
                    { 7, "Taxi" },
                    { 8, "Other Transport" },
                    { 9, "Food and Drinks" },
                    { 10, "Entertainment" },
                    { 11, "Souvenirs" },
                    { 12, "Visa or eTA" },
                    { 13, "Other Expenses" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_TravelEntities_CategoryId",
                table: "TravelEntities",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TravelEntities_CostCenterId",
                table: "TravelEntities",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_TravelEntities_StatusId",
                table: "TravelEntities",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_TravelSubEntities_CategoryId",
                table: "TravelSubEntities",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TravelSubEntities_TravelId",
                table: "TravelSubEntities",
                column: "TravelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TravelSubEntities");

            migrationBuilder.DropTable(
                name: "TravelEntities");

            migrationBuilder.DropTable(
                name: "TravelSubEntityCategories");

            migrationBuilder.DropTable(
                name: "TravelCategories");

            migrationBuilder.DropTable(
                name: "TravelCostCenters");

            migrationBuilder.DropTable(
                name: "TravelStatuses");
        }
    }
}
