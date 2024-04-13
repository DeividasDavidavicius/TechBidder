using System.ComponentModel.DataAnnotations;

namespace Backend_PcAuction.Data.Dtos
{
    public record PurchaseDto(Guid Id, double Amount, string Status, DateTime AuctionWinDate, DateTime? PaymentDate, string UserId, Guid AuctionId);
    public record PurchaseStripeDto(string Id, string Url);
}
