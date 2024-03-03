using System.ComponentModel.DataAnnotations;

namespace Backend_PcAuction.Data.Dtos
{
    public record AuctionDto(Guid Id, string Name, DateTime CreationDate, DateTime StartDate, DateTime EndDate);
    public record CreateAuctionDto([Required] string Name, [Required] DateTime StartDate, [Required] DateTime EndDate);
    public record UpdateAuctionDto([Required] string Name);
}
