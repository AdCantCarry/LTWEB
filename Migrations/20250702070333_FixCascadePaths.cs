using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechNova.Migrations
{
    /// <inheritdoc />
    public partial class FixCascadePaths : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SpecificationGroups",
                columns: table => new
                {
                    SpecificationGroupId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GroupName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecificationGroups", x => x.SpecificationGroupId);
                    table.ForeignKey(
                        name: "FK_SpecificationGroups_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpecificationItems",
                columns: table => new
                {
                    SpecificationItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    ItemName = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecificationItems", x => x.SpecificationItemId);
                    table.ForeignKey(
                        name: "FK_SpecificationItems_SpecificationGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "SpecificationGroups",
                        principalColumn: "SpecificationGroupId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductSpecifications",
                columns: table => new
                {
                    ProductSpecificationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    SpecificationItemId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ProductId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductSpecifications", x => x.ProductSpecificationId);
                    table.ForeignKey(
                        name: "FK_ProductSpecifications_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductSpecifications_Products_ProductId1",
                        column: x => x.ProductId1,
                        principalTable: "Products",
                        principalColumn: "ProductId");
                    table.ForeignKey(
                        name: "FK_ProductSpecifications_SpecificationItems_SpecificationItemId",
                        column: x => x.SpecificationItemId,
                        principalTable: "SpecificationItems",
                        principalColumn: "SpecificationItemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductSpecifications_ProductId",
                table: "ProductSpecifications",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductSpecifications_ProductId1",
                table: "ProductSpecifications",
                column: "ProductId1");

            migrationBuilder.CreateIndex(
                name: "IX_ProductSpecifications_SpecificationItemId",
                table: "ProductSpecifications",
                column: "SpecificationItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SpecificationGroups_CategoryId",
                table: "SpecificationGroups",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_SpecificationItems_GroupId",
                table: "SpecificationItems",
                column: "GroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductSpecifications");

            migrationBuilder.DropTable(
                name: "SpecificationItems");

            migrationBuilder.DropTable(
                name: "SpecificationGroups");
        }
    }
}
