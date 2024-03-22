using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend_PcAuction.Migrations
{
    /// <inheritdoc />
    public partial class Changedimagesavingtoazureblob : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageData",
                table: "Auctions");

            migrationBuilder.RenameColumn(
                name: "Picture",
                table: "Auctions",
                newName: "ImageUri");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageUri",
                table: "Auctions",
                newName: "Picture");

            migrationBuilder.AddColumn<byte[]>(
                name: "ImageData",
                table: "Auctions",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);
        }
    }
}
