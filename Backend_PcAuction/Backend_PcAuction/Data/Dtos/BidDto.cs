using System.ComponentModel.DataAnnotations;

namespace Backend_PcAuction.Data.Dtos
{
    public record BidDto(Guid Id, double Amount, DateTime CreationDate);
    public record CreateBidDto([Required] double Amount);
}
