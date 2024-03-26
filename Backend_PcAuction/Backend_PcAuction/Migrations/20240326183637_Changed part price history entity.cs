using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend_PcAuction.Migrations
{
    /// <inheritdoc />
    public partial class Changedpartpricehistoryentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PartPrices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<double>(type: "float", nullable: false),
                    PriceCheckDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PartId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartPrices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartPrices_Parts_PartId",
                        column: x => x.PartId,
                        principalTable: "Parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PartPrices_PartId",
                table: "PartPrices",
                column: "PartId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PartPrices");
        }
    }
}
