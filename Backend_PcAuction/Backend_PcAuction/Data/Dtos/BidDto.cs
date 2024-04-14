using System.ComponentModel.DataAnnotations;

namespace Backend_PcAuction.Data.Dtos
{
    public record BidDto(Guid Id, double Amount, DateTime CreationDate);
    public record BidWithUsernameDto(Guid Id, double Amount, DateTime CreationDate, string Username);

    public record CreateBidDto([Required] double Amount);
}
