using Backend_PcAuction.Data.Entities;
using Backend_PcAuction.Utils;
using Microsoft.EntityFrameworkCore;

namespace Backend_PcAuction.Data.Repositories
{
    public interface IPurchaseRepository
    {
        Task CreateAsync(Purchase purchase);
        Task DeleteAsync(Purchase purchase);
        Task<Purchase?> GetLastAsync(Guid auctionId);
        Task<IReadOnlyList<Purchase>> GetManyAsync(Guid auctionId);
        Task UpdateAsync(Purchase purchase);
        Task<IReadOnlyList<Purchase>> GetAllUserPurchasesAsync(string userId);
    }

    public class PurchaseRepository : IPurchaseRepository
    {
        private readonly PcAuctionDbContext _context;

        public PurchaseRepository(PcAuctionDbContext auctionDbContext)
        {
            _context = auctionDbContext;
        }

        public async Task CreateAsync(Purchase purchase)
        {
            _context.Purchases.Add(purchase);
            await _context.SaveChangesAsync();
        }

        public async Task<Purchase?> GetLastAsync(Guid auctionId)
        {
            return await _context.Purchases.Include(p => p.Buyer).Include(p => p.Auction).OrderByDescending(p => p.AuctionWinDate).
                FirstOrDefaultAsync(p => p.Auction.Id == auctionId);
        }

        public async Task<IReadOnlyList<Purchase>> GetManyAsync(Guid auctionId)
        {
            return await _context.Purchases.Where(p => p.Auction.Id == auctionId).OrderBy(p => p.AuctionWinDate).ToListAsync();
        }

        public async Task UpdateAsync(Purchase purchase)
        {
            _context.Purchases.Update(purchase);
            await _context.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<Purchase>> GetAllUserPurchasesAsync(string userId)
        {
            return await _context.Purchases.Include(p => p.Auction).Include(p => p.Auction.Part).Include(p => p.Auction.Part.Category).
                Where(p => p.Buyer.Id == userId).ToListAsync();
        }

        public async Task DeleteAsync(Purchase purchase)
        {
            _context.Purchases.Remove(purchase);
            await _context.SaveChangesAsync();
        }
    }
}
