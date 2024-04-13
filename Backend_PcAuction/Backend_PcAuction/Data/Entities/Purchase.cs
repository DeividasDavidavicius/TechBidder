using Backend_PcAuction.Auth.Model;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Backend_PcAuction.Data.Entities
{
    public class Purchase
    {
        public Guid Id { get; set; }
        public double Amount { get; set; }
        public string Status { get; set; }
        public DateTime AuctionWinDate { get; set; }
        public DateTime? PaymentDate { get; set; }
        public Auction Auction { get; set; }
        [Required]
        public string BuyerId { get; set; }
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public User Buyer { get; set; }
    }
}
