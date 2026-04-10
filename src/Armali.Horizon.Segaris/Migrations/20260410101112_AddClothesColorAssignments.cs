using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Armali.Horizon.Segaris.Migrations
{
    /// <inheritdoc />
    public partial class AddClothesColorAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClothesEntities_ClothesWashTypes_CategoryId",
                table: "ClothesEntities");

            migrationBuilder.CreateTable(
                name: "ClothesColors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Reference = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClothesColors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClothesColorStyles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClothesColorStyles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClothesColorAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GarmentId = table.Column<int>(type: "INTEGER", nullable: false),
                    ColorId = table.Column<int>(type: "INTEGER", nullable: false),
                    StyleId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClothesColorAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClothesColorAssignments_ClothesColorStyles_StyleId",
                        column: x => x.StyleId,
                        principalTable: "ClothesColorStyles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClothesColorAssignments_ClothesColors_ColorId",
                        column: x => x.ColorId,
                        principalTable: "ClothesColors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClothesColorAssignments_ClothesEntities_GarmentId",
                        column: x => x.GarmentId,
                        principalTable: "ClothesEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ClothesColorStyles",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Primary" },
                    { 2, "Secondary" },
                    { 3, "Details" }
                });

            migrationBuilder.InsertData(
                table: "ClothesColors",
                columns: new[] { "Id", "Name", "Reference" },
                values: new object[,]
                {
                    { 1, "Black", "#000000" },
                    { 2, "White", "#FFFFFF" },
                    { 3, "Gray", "#808080" },
                    { 4, "Dark Gray", "#404040" },
                    { 5, "Light Gray", "#C0C0C0" },
                    { 6, "Navy", "#000080" },
                    { 7, "Blue", "#0000FF" },
                    { 8, "Light Blue", "#87CEEB" },
                    { 9, "Red", "#FF0000" },
                    { 10, "Dark Red", "#8B0000" },
                    { 11, "Green", "#008000" },
                    { 12, "Olive", "#808000" },
                    { 13, "Yellow", "#FFD700" },
                    { 14, "Orange", "#FF8C00" },
                    { 15, "Brown", "#8B4513" },
                    { 16, "Beige", "#F5F5DC" },
                    { 17, "Pink", "#FF69B4" },
                    { 18, "Purple", "#800080" },
                    { 19, "Teal", "#008080" },
                    { 20, "Khaki", "#BDB76B" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClothesEntities_WashTypeId",
                table: "ClothesEntities",
                column: "WashTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ClothesColorAssignments_ColorId",
                table: "ClothesColorAssignments",
                column: "ColorId");

            migrationBuilder.CreateIndex(
                name: "IX_ClothesColorAssignments_GarmentId",
                table: "ClothesColorAssignments",
                column: "GarmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ClothesColorAssignments_StyleId",
                table: "ClothesColorAssignments",
                column: "StyleId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClothesEntities_ClothesWashTypes_WashTypeId",
                table: "ClothesEntities",
                column: "WashTypeId",
                principalTable: "ClothesWashTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClothesEntities_ClothesWashTypes_WashTypeId",
                table: "ClothesEntities");

            migrationBuilder.DropTable(
                name: "ClothesColorAssignments");

            migrationBuilder.DropTable(
                name: "ClothesColorStyles");

            migrationBuilder.DropTable(
                name: "ClothesColors");

            migrationBuilder.DropIndex(
                name: "IX_ClothesEntities_WashTypeId",
                table: "ClothesEntities");

            migrationBuilder.AddForeignKey(
                name: "FK_ClothesEntities_ClothesWashTypes_CategoryId",
                table: "ClothesEntities",
                column: "CategoryId",
                principalTable: "ClothesWashTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
