using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Armali.Horizon.Segaris.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectRisk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProjectRiskCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectRiskCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProjectRiskElements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    CategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    Probability = table.Column<int>(type: "INTEGER", nullable: false),
                    Severity = table.Column<int>(type: "INTEGER", nullable: false),
                    Mitigation = table.Column<int>(type: "INTEGER", nullable: false),
                    ProjectId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectRiskElements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectRiskElements_ProjectEntities_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "ProjectEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectRiskElements_ProjectRiskCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ProjectRiskCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ProjectRiskCategories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Technical" },
                    { 2, "Financial" },
                    { 3, "Schedule" },
                    { 4, "Scope" },
                    { 5, "Resource" },
                    { 6, "External" },
                    { 7, "Legal" },
                    { 8, "Operational" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRiskElements_CategoryId",
                table: "ProjectRiskElements",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRiskElements_ProjectId",
                table: "ProjectRiskElements",
                column: "ProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectRiskElements");

            migrationBuilder.DropTable(
                name: "ProjectRiskCategories");
        }
    }
}
