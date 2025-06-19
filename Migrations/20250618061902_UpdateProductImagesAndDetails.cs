using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechNova.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProductImagesAndDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Storage",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SubImage1Url",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SubImage2Url",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SubImage3Url",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Color",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Storage",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SubImage1Url",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SubImage2Url",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SubImage3Url",
                table: "Products");
        }
    }
}
