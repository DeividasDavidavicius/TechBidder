using System.ComponentModel.DataAnnotations;

namespace Backend_PcAuction.Data.Dtos
{
    public record AuctionDto(Guid Id, string Name, string Description, DateTime CreationDate, DateTime StartDate,
        DateTime EndDate, double MinIncrement, string Condition, string Manufacturer, string UserId);
    public record AuctionDtoWithImage(Guid Id, string Name, string Description, DateTime CreationDate, DateTime StartDate,
        DateTime EndDate, double MinIncrement, string Condition, string Manufacturer, string ImageData, string UserId);
    public record CreateAuctionDto([Required] string Name, [Required] string Description, [Required] DateTime StartDate,
        [Required] DateTime EndDate, [Required] [Range(0, double.MaxValue)] double MinIncrement, [Required] string Condition,
        string Manufacturer, [Required] string Picture, [Required] string ImageData);
    public record UpdateAuctionDto([Required] string Name, [Required] string Description, [Required] DateTime StartDate,
        [Required] DateTime EndDate, [Required][Range(0, double.MaxValue)] double MinIncrement, [Required] string Condition,
        string Manufacturer, [Required] string Picture, [Required] string ImageData);
    public record AuctionsWithPaginationDto(IEnumerable<AuctionDtoWithImage> Auctions, int AuctionCount);
}
