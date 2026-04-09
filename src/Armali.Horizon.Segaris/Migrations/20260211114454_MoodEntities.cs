using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Armali.Horizon.Segaris.Migrations
{
    /// <inheritdoc />
    public partial class MoodEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MoodCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    PrimaryColor = table.Column<string>(type: "TEXT", nullable: false),
                    SecondaryColor = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MoodCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MoodEntities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Score = table.Column<int>(type: "INTEGER", nullable: false),
                    CategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsPrivate = table.Column<bool>(type: "INTEGER", nullable: false),
                    Creator = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MoodEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MoodEntities_MoodCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "MoodCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "MoodCategories",
                columns: new[] { "Id", "Name", "PrimaryColor", "SecondaryColor" },
                values: new object[,]
                {
                    { 1, "Happy", "green", "green" },
                    { 2, "  Excited", "green", "sky" },
                    { 3, "  Optimistic", "green", "azure" },
                    { 4, "  Grateful", "green", "pink" },
                    { 5, "  Vibing", "green", "viridian" },
                    { 6, "Playful", "sky", "sky" },
                    { 7, "  Flirty", "sky", "pink" },
                    { 8, "  Cheeky", "sky", "azure" },
                    { 9, "  Energetic", "sky", "spring" },
                    { 10, "  Funny", "sky", "green" },
                    { 11, "Focused", "teal", "teal" },
                    { 12, "  Productive", "teal", "azure" },
                    { 13, "  Absorbed", "teal", "viridian" },
                    { 14, "  Flowing", "teal", "spring" },
                    { 15, "  Withdrawn", "teal", "cobalt" },
                    { 16, "Love", "pink", "pink" },
                    { 17, "  Affectionate", "pink", "sky" },
                    { 18, "  Caring", "pink", "teal" },
                    { 19, "  Safe", "pink", "viridian" },
                    { 20, "  Connected", "pink", "green" },
                    { 21, "Confident", "azure", "azure" },
                    { 22, "  Empowered", "azure", "green" },
                    { 23, "  Proud", "azure", "pink" },
                    { 24, "  Determined", "azure", "teal" },
                    { 25, "  Bold", "azure", "spring" },
                    { 26, "Inspired", "spring", "spring" },
                    { 27, "  Curious", "spring", "teal" },
                    { 28, "  Daring", "spring", "azure" },
                    { 29, "  Amazed", "spring", "green" },
                    { 30, "  Creative", "spring", "sky" },
                    { 31, "Peaceful", "viridian", "viridian" },
                    { 32, "  Relaxed", "viridian", "azure" },
                    { 33, "  Satisfied", "viridian", "green" },
                    { 34, "  Relieved", "viridian", "teal" },
                    { 35, "  Serene", "viridian", "spring" },
                    { 36, "Lazy", "gray", "gray" },
                    { 37, "  Relaxed", "gray", "azure" },
                    { 38, "  Satified", "gray", "green" },
                    { 39, "  Relieved", "gray", "teal" },
                    { 40, "  Serene", "gray", "spring" },
                    { 41, "Fine", "white", "white" },
                    { 42, "Insecure", "orange", "orange" },
                    { 43, "  Ashamed", "orange", "red" },
                    { 44, "  Worthless", "orange", "cobalt" },
                    { 45, "  Doubtful", "orange", "brown" },
                    { 46, "  Paranoid", "orange", "indigo" },
                    { 47, "Angry", "red", "red" },
                    { 48, "  Frustrated", "red", "brown" },
                    { 49, "  Bitter", "red", "violet" },
                    { 50, "  Resentful", "red", "cobalt" },
                    { 51, "  Furious", "red", "yellow" },
                    { 52, "Depleted", "purple", "purple" },
                    { 53, "  Tired", "purple", "viridian" },
                    { 54, "  Burnout", "purple", "violet" },
                    { 55, "  Apathetic", "purple", "indigo" },
                    { 56, "  Numb", "purple", "white" },
                    { 57, "Uncomfortable", "yellow", "yellow" },
                    { 58, "  Disgusted", "yellow", "red" },
                    { 59, "  Jealous", "yellow", "pink" },
                    { 60, "  Embarrased", "yellow", "orange" },
                    { 61, "  Awkward", "yellow", "brown" },
                    { 62, "Tense", "violet", "violet" },
                    { 63, "  Anxious", "violet", "yellow" },
                    { 64, "  Overwhelmed", "violet", "red" },
                    { 65, "  Restless", "violet", "purple" },
                    { 66, "  Wary", "violet", "brown" },
                    { 67, "Confused", "brown", "brown" },
                    { 68, "  Indecisive", "brown", "orange" },
                    { 69, "  Lost", "brown", "indigo" },
                    { 70, "  Surprised", "brown", "yellow" },
                    { 71, "  Betrayed", "brown", "red" },
                    { 72, "Sad", "cobalt", "cobalt" },
                    { 73, "  Disappointed", "cobalt", "brown" },
                    { 74, "  Lonely", "cobalt", "viridian" },
                    { 75, "  Heartbroken", "cobalt", "pink" },
                    { 76, "  Nostalgic", "cobalt", "spring" },
                    { 77, "Scared", "indigo", "indigo" },
                    { 78, "  Startled", "indigo", "brown" },
                    { 79, "  Worried", "indigo", "violet" },
                    { 80, "  Suspicious", "indigo", "orange" },
                    { 81, "  Pesimistic", "indigo", "cobalt" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_MoodEntities_CategoryId",
                table: "MoodEntities",
                column: "CategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MoodEntities");

            migrationBuilder.DropTable(
                name: "MoodCategories");
        }
    }
}
