using Backend_PcAuction.Data.Entities;
using Backend_PcAuction.Utils;
using Microsoft.EntityFrameworkCore;

namespace Backend_PcAuction.Data.Repositories
{
    public interface IAuctionsRepository
    {
        Task CreateAsync(Auction auction);
        Task DeleteAsync(Auction auction);
        Task<Auction?> GetAsync(Guid auctionId);
        Task<IReadOnlyList<Auction>> GetManyAsync();
        Task<List<Auction>> GetManyByPartAsync(Auction auction);
        Task<List<Auction>> GetManyBySeriesDifferentPartAsync(Auction auction);
        Task<List<Auction>> GetManyByCategoryDifferentSeriesAsync(Auction auction);
        Task<List<Auction>> GetManyByCategoryDifferentPartAsync(Auction auction);
        Task<IReadOnlyList<Auction>> GetManyWithPaginationAsync(int page);
        Task UpdateAsync(Auction auction);
        Task<int> GetCountAsync();
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
            return await _context.Auctions.Include(a => a.Part).Include(a => a.Part.Category).Include(a => a.Part.Series).FirstOrDefaultAsync(a => a.Id == auctionId);
        }

        public async Task<IReadOnlyList<Auction>> GetManyAsync()
        {
            return await _context.Auctions.Include(a => a.Part).Include(a => a.Part.Category).ToListAsync();
        }

        public async Task<List<Auction>> GetManyByPartAsync(Auction auction)
        {
            return await _context.Auctions.Include(a => a.Part).Include(a => a.Part.Category).Where(a => a.Status == AuctionStatuses.Active && a.Part.Id == auction.Part.Id && a.Id != auction.Id).ToListAsync();
        }

        public async Task<List<Auction>> GetManyBySeriesDifferentPartAsync(Auction auction)
        {
            return await _context.Auctions.Include(a => a.Part).Include(a => a.Part.Category).Where(a => a.Status == AuctionStatuses.Active && a.Part.Series.Id == auction.Part.Series.Id && a.Part.Id != auction.Part.Id).ToListAsync();
        }

        public async Task<List<Auction>> GetManyByCategoryDifferentSeriesAsync(Auction auction)
        {
            return await _context.Auctions.Include(a => a.Part).Include(a => a.Part.Category).Where(a => a.Status == AuctionStatuses.Active && a.Part.Category.Id == auction.Part.Category.Id && a.Part.Series.Id != auction.Part.Series.Id && a.Part.Id != auction.Part.Id).ToListAsync();
        }

        public async Task<List<Auction>> GetManyByCategoryDifferentPartAsync(Auction auction)
        {
            return await _context.Auctions.Include(a => a.Part).Include(a => a.Part.Category).Where(a => a.Status == AuctionStatuses.Active && a.Part.Category.Id == auction.Part.Category.Id && a.Part.Id != auction.Part.Id).ToListAsync();
        }

        public async Task<IReadOnlyList<Auction>> GetManyWithPaginationAsync(int page)
        {
            return await _context.Auctions.Include(a => a.Part).Include(a => a.Part.Category).Where(a => a.Status == AuctionStatuses.Active).Skip((page - 1) * 5).Take(5).ToListAsync();
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

        public async Task<int> GetCountAsync()
        {
            return await _context.Auctions.Where(a => a.Status == AuctionStatuses.Active).CountAsync();
        }
    }
}
