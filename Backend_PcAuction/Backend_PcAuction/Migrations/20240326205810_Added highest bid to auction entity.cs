using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend_PcAuction.Migrations
{
    /// <inheritdoc />
    public partial class Addedhighestbidtoauctionentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "HighestBid",
                table: "Auctions",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HighestBid",
                table: "Auctions");
        }
    }
}
