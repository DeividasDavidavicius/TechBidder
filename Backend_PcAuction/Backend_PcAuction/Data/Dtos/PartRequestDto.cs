using System.ComponentModel.DataAnnotations;

namespace Backend_PcAuction.Data.Dtos
{
    public record PartRequestDto(Guid Id, string Name, Guid AuctionId, Guid PartId, string CategoryId);
    public record CreatePartRequestDto([Required] string Name, [Required] Guid AuctionId, [Required] string CategoryId, [Required] Guid PartId);
}
