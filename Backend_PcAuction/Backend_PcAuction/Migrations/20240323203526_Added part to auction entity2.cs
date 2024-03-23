using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend_PcAuction.Migrations
{
    /// <inheritdoc />
    public partial class Addedparttoauctionentity2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PartId",
                table: "Auctions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Auctions_PartId",
                table: "Auctions",
                column: "PartId");

            migrationBuilder.AddForeignKey(
                name: "FK_Auctions_Parts_PartId",
                table: "Auctions",
                column: "PartId",
                principalTable: "Parts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Auctions_Parts_PartId",
                table: "Auctions");

            migrationBuilder.DropIndex(
                name: "IX_Auctions_PartId",
                table: "Auctions");

            migrationBuilder.DropColumn(
                name: "PartId",
                table: "Auctions");
        }
    }
}
