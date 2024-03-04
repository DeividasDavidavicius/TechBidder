using Backend_PcAuction.Auth.Model;
using Backend_PcAuction.Auth.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
 
namespace Backend_PcAuction.Data.Entities
{
    public class Auction : IUserOwnedResource
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        [Required]
        public string UserId { get; set; }
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public User User { get; set; } 
    }
}
