using System.ComponentModel.DataAnnotations;

namespace Backend_PcAuction.Data.Dtos
{
    public record PurchaseDto(Guid Id, string Status, DateTime AuctionWinDate, DateTime? PaymentDate, string UserId, Guid AuctionId);
}
