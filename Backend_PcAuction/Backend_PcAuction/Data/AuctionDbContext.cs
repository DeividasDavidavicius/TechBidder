using Backend_PcAuction.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Backend_PcAuction.Data
{
    public class AuctionDbContext : DbContext
    {
        public DbSet<Auction> Auctions { get; set; }
        public DbSet<Bid> Bids { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB; Initial Catalog=PcAuctionDb");
        }
    }
}
