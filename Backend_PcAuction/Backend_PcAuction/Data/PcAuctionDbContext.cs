using Backend_PcAuction.Auth.Model;
using Backend_PcAuction.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Backend_PcAuction.Data
{
    public class PcAuctionDbContext : IdentityDbContext<User>
    {
        public DbSet<Auction> Auctions { get; set; }
        public DbSet<Bid> Bids { get; set; }
        public DbSet<Part> Parts { get; set; }
        public DbSet<PartCategory > PartCategories { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB; Initial Catalog=PcAuctionDb");
        }
    }
}
