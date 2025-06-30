using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechNova.Migrations
{
    /// <inheritdoc />
    public partial class AddResetcode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResetToken",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "ResetTokenExpiry",
                table: "Users",
                newName: "ResetCodeExpiry");

            migrationBuilder.AddColumn<string>(
                name: "ResetCode",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResetCode",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "ResetCodeExpiry",
                table: "Users",
                newName: "ResetTokenExpiry");

            migrationBuilder.AddColumn<string>(
                name: "ResetToken",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
