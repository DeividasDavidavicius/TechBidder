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
        public string Description { get; set; }
        public DateTime CreationDate { get; set; } // TODO Gal saugot UTC laika visur, o fronte paconvertint i local (UtcNow)
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double MinIncrement { get; set; }
        public string Status { get; set; }
        // add part
        public string Condition { get; set; }
        public string Manufacturer { get; set; }
        public string Picture { get; set; } // del rodymo paziuret kaip skelbiu ratio padaro

        [Required]
        public string UserId { get; set; }
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public User User { get; set; } 
    }
}
