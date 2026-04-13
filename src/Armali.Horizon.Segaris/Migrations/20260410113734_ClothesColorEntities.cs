using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Armali.Horizon.Segaris.Migrations
{
    /// <inheritdoc />
    public partial class ClothesColorEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ClothesColors",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Name", "Reference" },
                values: new object[] { "Maroon", "#800000" });

            migrationBuilder.UpdateData(
                table: "ClothesColors",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Name", "Reference" },
                values: new object[] { "Auburn", "#D22C21" });

            migrationBuilder.UpdateData(
                table: "ClothesColors",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Name", "Reference" },
                values: new object[] { "Coral", "#E51D2E" });

            migrationBuilder.UpdateData(
                table: "ClothesColors",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Name", "Reference" },
                values: new object[] { "Vermilion", "#E62E00" });

            migrationBuilder.UpdateData(
                table: "ClothesColors",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Name", "Reference" },
                values: new object[] { "Indian", "#FF8000" });

            migrationBuilder.UpdateData(
                table: "ClothesColors",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Name", "Reference" },
                values: new object[] { "Flame", "#F98F1D" });

            migrationBuilder.UpdateData(
                table: "ClothesColors",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Name", "Reference" },
                values: new object[] { "Amber", "#FFBF00" });

            migrationBuilder.UpdateData(
                table: "ClothesColors",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Name", "Reference" },
                values: new object[] { "Hansa", "#FFDF00" });

            migrationBuilder.UpdateData(
                table: "ClothesColors",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Name", "Reference" },
                values: new object[] { "Lime", "#D9E542" });

            migrationBuilder.UpdateData(
                table: "ClothesColors",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Name", "Reference" },
                values: new object[] { "Chartreuse", "#75B313" });

            migrationBuilder.UpdateData(
                table: "ClothesColors",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "Name", "Reference" },
                values: new object[] { "Shamrock", "#009E60" });

            migrationBuilder.UpdateData(
                table: "ClothesColors",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "Name", "Reference" },
                values: new object[] { "Viridian", "#007F5C" });

            migrationBuilder.UpdateData(
                table: "ClothesColors",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "Name", "Reference" },
                values: new object[] { "Emerald", "#3FD8AA" });

            migrationBuilder.UpdateData(
                table: "ClothesColors",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "Name", "Reference" },
                values: new object[] { "Verdigris", "#43B3AE" });

            migrationBuilder.UpdateData(
                table: "ClothesColors",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "Name", "Reference" },
                values: new object[] { "Munsell", "#367588" });

            migrationBuilder.UpdateData(
                table: "ClothesColors",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "Name", "Reference" },
                values: new object[] { "Sky", "#0CB7F2" });

            migrationBuilder.UpdateData(
                table: "ClothesColors",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "Name", "Reference" },
                values: new object[] { "Cerulean", "#0087D1" });

            migrationBuilder.UpdateData(
                table: "ClothesColors",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "Name", "Reference" },
                values: new object[] { "Indigo", "#0A3F7A" });

            migrationBuilder.UpdateData(
                table: "ClothesColors",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "Name", "Reference" },
                values: new object[] { "Cobalt", "#333C87" });

            migrationBuilder.UpdateData(
                table: "ClothesColors",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "Name", "Reference" },
                values: new object[] { "Violet", "#4C2882" });

            migrationBuilder.InsertData(
                table: "ClothesColors",
                columns: new[] { "Id", "Name", "Reference" },
                values: new object[,]
                {
                    { 21, "Iris", "#7F68A5" },
                    { 22, "Mauve", "#E0B0FF" },
                    { 23, "Orcein", "#C20073" },
                    { 24, "Salmon", "#EB6362" },
                    { 25, "Sandy", "#ECE2C6" },
                    { 26, "Sienna", "#C58A3E" },
                    { 27, "Cinnamon", "#8D4925" },
                    { 28, "Umber", "#955F20" },
                    { 29, "Chestnut", "#5D432C" },
                    { 30, "Sepia", "#663B2A" },
                    { 31, "Artemisia", "#E0E5FF" },
                    { 32, "Ash", "#CDCDCD" },
                    { 33, "Steel", "#8B8589" },
                    { 34, "Slate", "#5D6770" },
                    { 35, "Anthracite", "#383E42" },
                    { 36, "Cordovan", "#3B2A21" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ClothesColors",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "ClothesColors",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "ClothesColors",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "ClothesColors",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "ClothesColors",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "ClothesColors",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "ClothesColors",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "ClothesColors",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "ClothesColors",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "ClothesColors",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "ClothesColors",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "ClothesColors",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "ClothesColors",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "ClothesColors",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "ClothesColors",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "ClothesColors",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.UpdateData(
                table: "ClothesColors",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Name", "Reference" },
                values: new object[] { "Black", "#000000" });

            migrationBuilder.UpdateData(
                table: "ClothesColors",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Name", "Reference" },
                values: new object[] { "White", "#FFFFFF" });

            migrationBuilder.UpdateData(
                table: "ClothesColors",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Name", "Reference" },
                values: new object[] { "Gray", "#808080" });

            migrationBuilder.UpdateData(
                table: "ClothesColors",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Name", "Reference" },
                values: new object[] { "Dark Gray", "#404040" });

            migrationBuilder.UpdateData(
                table: "ClothesColors",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Name", "Reference" },
                values: new object[] { "Light Gray", "#C0C0C0" });

            migrationBuilder.UpdateData(
                table: "ClothesColors",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Name", "Reference" },
                values: new object[] { "Navy", "#000080" });

            migrationBuilder.UpdateData(
                table: "ClothesColors",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Name", "Reference" },
                values: new object[] { "Blue", "#0000FF" });

            migrationBuilder.UpdateData(
                table: "ClothesColors",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Name", "Reference" },
                values: new object[] { "Light Blue", "#87CEEB" });

            migrationBuilder.UpdateData(
                table: "ClothesColors",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Name", "Reference" },
                values: new object[] { "Red", "#FF0000" });

            migrationBuilder.UpdateData(
                table: "ClothesColors",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Name", "Reference" },
                values: new object[] { "Dark Red", "#8B0000" });

            migrationBuilder.UpdateData(
                table: "ClothesColors",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "Name", "Reference" },
                values: new object[] { "Green", "#008000" });

            migrationBuilder.UpdateData(
                table: "ClothesColors",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "Name", "Reference" },
                values: new object[] { "Olive", "#808000" });

            migrationBuilder.UpdateData(
                table: "ClothesColors",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "Name", "Reference" },
                values: new object[] { "Yellow", "#FFD700" });

            migrationBuilder.UpdateData(
                table: "ClothesColors",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "Name", "Reference" },
                values: new object[] { "Orange", "#FF8C00" });

            migrationBuilder.UpdateData(
                table: "ClothesColors",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "Name", "Reference" },
                values: new object[] { "Brown", "#8B4513" });

            migrationBuilder.UpdateData(
                table: "ClothesColors",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "Name", "Reference" },
                values: new object[] { "Beige", "#F5F5DC" });

            migrationBuilder.UpdateData(
                table: "ClothesColors",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "Name", "Reference" },
                values: new object[] { "Pink", "#FF69B4" });

            migrationBuilder.UpdateData(
                table: "ClothesColors",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "Name", "Reference" },
                values: new object[] { "Purple", "#800080" });

            migrationBuilder.UpdateData(
                table: "ClothesColors",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "Name", "Reference" },
                values: new object[] { "Teal", "#008080" });

            migrationBuilder.UpdateData(
                table: "ClothesColors",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "Name", "Reference" },
                values: new object[] { "Khaki", "#BDB76B" });
        }
    }
}
