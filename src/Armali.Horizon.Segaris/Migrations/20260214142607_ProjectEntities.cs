using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Armali.Horizon.Segaris.Migrations
{
    /// <inheritdoc />
    public partial class ProjectEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProjectAxis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    ProgramId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectAxis", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProjectPrograms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectPrograms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProjectStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Color = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProjectSubEntityCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectSubEntityCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProjectEntities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    ProgramId = table.Column<int>(type: "INTEGER", nullable: false),
                    AxisId = table.Column<int>(type: "INTEGER", nullable: false),
                    StatusId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsPrivate = table.Column<bool>(type: "INTEGER", nullable: false),
                    Creator = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectEntities_ProjectAxis_AxisId",
                        column: x => x.AxisId,
                        principalTable: "ProjectAxis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectEntities_ProjectPrograms_ProgramId",
                        column: x => x.ProgramId,
                        principalTable: "ProjectPrograms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectEntities_ProjectStatuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "ProjectStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectSubEntities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    File = table.Column<string>(type: "TEXT", nullable: false),
                    CategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProjectId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectSubEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectSubEntities_ProjectEntities_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "ProjectEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectSubEntities_ProjectSubEntityCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ProjectSubEntityCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ProjectAxis",
                columns: new[] { "Id", "Name", "ProgramId" },
                values: new object[,]
                {
                    { 1, "DEVL", 1 },
                    { 2, "MODS", 1 },
                    { 3, "MUSI", 1 },
                    { 4, "TVMV", 1 },
                    { 5, "VGME", 1 },
                    { 6, "ARTS", 2 },
                    { 7, "BOOK", 2 },
                    { 8, "BRGM", 2 },
                    { 9, "MISC", 2 },
                    { 10, "HOLI", 3 },
                    { 11, "VEHI", 3 },
                    { 12, "WMAP", 3 },
                    { 13, "WORK", 3 },
                    { 14, "EQPM", 4 },
                    { 15, "FUNC", 4 },
                    { 16, "FURN", 4 },
                    { 17, "STRU", 4 },
                    { 18, "DOCU", 5 },
                    { 19, "GOVM", 5 },
                    { 20, "OPEX", 5 },
                    { 21, "PROC", 5 },
                    { 22, "AGEN", 6 },
                    { 23, "AZUR", 6 },
                    { 24, "CTRL", 6 },
                    { 25, "EXTR", 6 },
                    { 26, "MGMT", 6 },
                    { 27, "NETW", 6 },
                    { 28, "FIRE", 7 },
                    { 29, "GRUP", 7 },
                    { 30, "SIPL", 7 }
                });

            migrationBuilder.InsertData(
                table: "ProjectPrograms",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "DIGI" },
                    { 2, "ENTR" },
                    { 3, "EXPL" },
                    { 4, "HOME" },
                    { 5, "LOGI" },
                    { 6, "PLAT" },
                    { 7, "SOCI" }
                });

            migrationBuilder.InsertData(
                table: "ProjectStatuses",
                columns: new[] { "Id", "Color", "Name" },
                values: new object[,]
                {
                    { 1, "blue", "Planning" },
                    { 2, "gold", "Active" },
                    { 3, "gray", "Paused" },
                    { 4, "green", "Completed" },
                    { 5, "red", "Closed" }
                });

            migrationBuilder.InsertData(
                table: "ProjectSubEntityCategories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Project Documentation" },
                    { 2, "Risk Analysis" },
                    { 3, "Contract" },
                    { 4, "Invoice" },
                    { 5, "Other Documentation" },
                    { 6, "Project Output" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectEntities_AxisId",
                table: "ProjectEntities",
                column: "AxisId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectEntities_ProgramId",
                table: "ProjectEntities",
                column: "ProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectEntities_StatusId",
                table: "ProjectEntities",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectSubEntities_CategoryId",
                table: "ProjectSubEntities",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectSubEntities_ProjectId",
                table: "ProjectSubEntities",
                column: "ProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectSubEntities");

            migrationBuilder.DropTable(
                name: "ProjectEntities");

            migrationBuilder.DropTable(
                name: "ProjectSubEntityCategories");

            migrationBuilder.DropTable(
                name: "ProjectAxis");

            migrationBuilder.DropTable(
                name: "ProjectPrograms");

            migrationBuilder.DropTable(
                name: "ProjectStatuses");
        }
    }
}
