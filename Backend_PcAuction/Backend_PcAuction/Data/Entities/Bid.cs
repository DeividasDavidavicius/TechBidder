using Backend_PcAuction.Auth.Model;
using Backend_PcAuction.Auth.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Backend_PcAuction.Data.Entities
{
    public class Bid : IUserOwnedResource
    {
        public Guid Id { get; set; }
        public double Amount { get; set; }
        public DateTime CreationDate { get; set; } // TODO Gal saugot UTC laika visur, o fronte paconvertint i local (UtcNow)
        public Auction Auction { get; set; }
        [Required]
        public string UserId { get; set; }
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public User User { get; set; }
    }
}
