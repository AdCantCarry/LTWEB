using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechNova.Migrations
{
    /// <inheritdoc />
    public partial class AddMainImageUrlToCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MainImageUrl",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MainImageUrl",
                table: "Categories");
        }
    }
}
