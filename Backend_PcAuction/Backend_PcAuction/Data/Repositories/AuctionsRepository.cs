using Backend_PcAuction.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend_PcAuction.Data.Repositories
{
    public interface IAuctionsRepository
    {
        Task CreateAsync(Auction auction);
        Task DeleteAsync(Auction auction);
        Task<Auction?> GetAsync(Guid auctionId);
        Task<IReadOnlyList<Auction>> GetManyAsync();
        Task UpdateAsync(Auction auction);
    }

    public class AuctionsRepository : IAuctionsRepository
    {
        private readonly PcAuctionDbContext _context;

        public AuctionsRepository(PcAuctionDbContext auctionDbContext)
        {
            _context = auctionDbContext;
        }

        public async Task CreateAsync(Auction auction)
        {
            _context.Auctions.Add(auction);
            await _context.SaveChangesAsync();
        }

        public async Task<Auction?> GetAsync(Guid auctionId)
        {
            return await _context.Auctions.FirstOrDefaultAsync(a => a.Id == auctionId);
        }

        public async Task<IReadOnlyList<Auction>> GetManyAsync()
        {
            return await _context.Auctions.ToListAsync();
        }

        public async Task UpdateAsync(Auction auction)
        {
            _context.Auctions.Update(auction);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Auction auction)
        {
            _context.Auctions.Remove(auction);
            await _context.SaveChangesAsync();
        }
    }
}
