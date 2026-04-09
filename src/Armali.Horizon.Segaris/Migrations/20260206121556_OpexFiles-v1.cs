using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Armali.Horizon.Segaris.Migrations
{
    /// <inheritdoc />
    public partial class OpexFilesv1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OpexCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpexCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OpexStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Color = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpexStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OpexEntities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    CategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    StatusId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsPrivate = table.Column<bool>(type: "INTEGER", nullable: false),
                    Creator = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpexEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpexEntities_OpexCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "OpexCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OpexEntities_OpexStatuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "OpexStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OpexSubEntities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Amount = table.Column<double>(type: "REAL", nullable: false),
                    ContractId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpexSubEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpexSubEntities_OpexEntities_ContractId",
                        column: x => x.ContractId,
                        principalTable: "OpexEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "OpexCategories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Government" },
                    { 2, "Employment" },
                    { 3, "Investment" },
                    { 4, "Software" },
                    { 5, "Entertainment" },
                    { 6, "Vehicles" },
                    { 7, "Facilities" }
                });

            migrationBuilder.InsertData(
                table: "OpexStatuses",
                columns: new[] { "Id", "Color", "Name" },
                values: new object[,]
                {
                    { 1, "blue", "Planning" },
                    { 2, "green", "Active" },
                    { 3, "gold", "Paused" },
                    { 4, "red", "Closed" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_OpexEntities_CategoryId",
                table: "OpexEntities",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_OpexEntities_StatusId",
                table: "OpexEntities",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_OpexSubEntities_ContractId",
                table: "OpexSubEntities",
                column: "ContractId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OpexSubEntities");

            migrationBuilder.DropTable(
                name: "OpexEntities");

            migrationBuilder.DropTable(
                name: "OpexCategories");

            migrationBuilder.DropTable(
                name: "OpexStatuses");
        }
    }
}
