using System.ComponentModel.DataAnnotations;

namespace Backend_PcAuction.Data.Dtos
{
    public record AuctionDto(
        Guid Id,
        string Name,
        string Description,
        DateTime CreationDate,
        DateTime StartDate,
        DateTime EndDate,
        double MinIncrement,
        string Condition,
        string Manufacturer,
        string ImageUri,
        string UserId,
        Guid PartId
    );
    
    public record CreateAuctionDto(
        [Required]string Name,
        [Required] string Description,
        [Required] DateTime StartDate,
        [Required] DateTime EndDate,
        [Required] [Range(0, double.MaxValue)] double MinIncrement,
        [Required] string Condition,
        string? Manufacturer,
        [Required] IFormFile? Image,
        string PartCategory,
        Guid PartId
    );
    
    public record UpdateAuctionDto(
        [Required] string Name,
        [Required] string Description,
        [Required] DateTime StartDate,
        [Required] DateTime EndDate,
        [Required][Range(0, double.MaxValue)] double MinIncrement,
        [Required] string Condition,
        string? Manufacturer,
        IFormFile? Image
    );
    
    public record AuctionsWithPaginationDto(
        IEnumerable<AuctionDto> Auctions,
        int AuctionCount
    );
}
