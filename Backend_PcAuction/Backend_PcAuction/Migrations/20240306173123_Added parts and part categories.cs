using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend_PcAuction.Migrations
{
    /// <inheritdoc />
    public partial class Addedpartsandpartcategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PartCategories",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SpecificationName1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpecificationName2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpecificationName3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpecificationName4 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpecificationName5 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpecificationName6 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpecificationName7 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpecificationName8 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpecificationName9 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpecificationName10 = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Parts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SpecificationValue1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpecificationValue2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpecificationValue3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpecificationValue4 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpecificationValue5 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpecificationValue6 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpecificationValue7 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpecificationValue8 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpecificationValue9 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpecificationValue10 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CategoryId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Parts_PartCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "PartCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Parts_CategoryId",
                table: "Parts",
                column: "CategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Parts");

            migrationBuilder.DropTable(
                name: "PartCategories");
        }
    }
}
