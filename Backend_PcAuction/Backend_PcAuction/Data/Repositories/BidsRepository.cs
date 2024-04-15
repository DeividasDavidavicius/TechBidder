using Backend_PcAuction.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend_PcAuction.Data.Repositories
{
    public interface IBidsRepository
    {
        Task CreateAsync(Bid bid);
        Task DeleteAsync(Bid bid);
        Task<Bid?> GetAsync(Guid auctionId, Guid bidId);
        Task<Bid?> GetLastAsync(Guid auctionId);
        Task<IReadOnlyList<Bid>> GetManyAsync(Guid auctionId);
    }

    public class BidsRepository : IBidsRepository
    {
        private readonly PcAuctionDbContext _context;

        public BidsRepository(PcAuctionDbContext auctionDbContext)
        {
            _context = auctionDbContext;
        }

        public async Task CreateAsync(Bid bid)
        {
            _context.Bids.Add(bid);
            await _context.SaveChangesAsync();
        }

        public async Task<Bid?> GetAsync(Guid auctionId, Guid bidId)
        {
            return await _context.Bids.FirstOrDefaultAsync(bid => bid.Id == bidId && bid.Auction.Id == auctionId);
        }

        public async Task<IReadOnlyList<Bid>> GetManyAsync(Guid auctionId)
        {
            return await _context.Bids.Include(bid => bid.User).Where(bid => bid.Auction.Id == auctionId).OrderByDescending(bid => bid.Amount).ToListAsync();
        }

        public async Task<Bid?> GetLastAsync(Guid auctionId)
        {
            var sortedSequence = _context.Bids.OrderByDescending(item => item.Amount);
            return await sortedSequence.FirstOrDefaultAsync(bid => bid.Auction.Id == auctionId);
        }

        public async Task DeleteAsync(Bid bid)
        {
            _context.Bids.Remove(bid);
            await _context.SaveChangesAsync();
        }
    }
}
