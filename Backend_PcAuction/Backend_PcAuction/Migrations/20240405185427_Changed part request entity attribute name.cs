using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend_PcAuction.Migrations
{
    /// <inheritdoc />
    public partial class Changedpartrequestentityattributename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PartRequests_Parts_TemporaryPartId",
                table: "PartRequests");

            migrationBuilder.RenameColumn(
                name: "TemporaryPartId",
                table: "PartRequests",
                newName: "PartId");

            migrationBuilder.RenameIndex(
                name: "IX_PartRequests_TemporaryPartId",
                table: "PartRequests",
                newName: "IX_PartRequests_PartId");

            migrationBuilder.AddForeignKey(
                name: "FK_PartRequests_Parts_PartId",
                table: "PartRequests",
                column: "PartId",
                principalTable: "Parts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PartRequests_Parts_PartId",
                table: "PartRequests");

            migrationBuilder.RenameColumn(
                name: "PartId",
                table: "PartRequests",
                newName: "TemporaryPartId");

            migrationBuilder.RenameIndex(
                name: "IX_PartRequests_PartId",
                table: "PartRequests",
                newName: "IX_PartRequests_TemporaryPartId");

            migrationBuilder.AddForeignKey(
                name: "FK_PartRequests_Parts_TemporaryPartId",
                table: "PartRequests",
                column: "TemporaryPartId",
                principalTable: "Parts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
