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
        string Status,
        string UserId,
        Guid PartId,
        string CategoryId
    );

    public record AuctionWithPartNameDto(
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
        string Status,
        string UserId,
        string PartName,
        string CategoryId
    );

    public record AuctionWithAvgPriceDto(
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
    string Status,
    double AveragePrice,
    string UserId,
    string PartName,
    string CategoryId
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
        Guid? PartId,
        string? PartName,
        string? PartCategoryName
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

    public record UpdateAuctionPartDto(
        string? CategoryId,
        Guid? PartId
    );
    
    public record AuctionsWithPaginationDto(
        IEnumerable<AuctionWithPartNameDto> Auctions,
        int AuctionCount
    );
}
