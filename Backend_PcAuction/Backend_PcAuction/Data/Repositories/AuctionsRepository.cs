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
        Task<Auction?> GetWithPartActiveCheapestAsync(Guid partId);
        Task<IReadOnlyList<Auction>> GetManyAsync();
        Task<List<Auction>> GetManyByPartAsync(Auction auction);
        Task<List<Auction>> GetManyBySeriesDifferentPartAsync(Auction auction);
        Task<List<Auction>> GetManyByCategoryDifferentSeriesAsync(Auction auction);
        Task<List<Auction>> GetManyByCategoryDifferentPartAsync(Auction auction);
        Task<IReadOnlyList<Auction>> GetManyWithPaginationAsync(int page, string categoryId, Guid? seriesId, Guid? partId, string sortType);
        Task<IReadOnlyList<Auction>> GetAllNewByUserAsync(string userId);
        Task<IReadOnlyList<Auction>> GetAllActiveByUserAsync(string userId);
        Task<IReadOnlyList<Auction>> GetAllEndedByUserAsync(string userId);
        Task UpdateAsync(Auction auction);
        Task<int> GetCountAsync(string categoryId, Guid? seriesId, Guid? partId);

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
            return await _context.Auctions.Include(a => a.Part).Include(a => a.Part.Category).Include(a => a.Part.Series).
                FirstOrDefaultAsync(a => a.Id == auctionId);
        }

        public async Task<Auction?> GetWithPartActiveCheapestAsync(Guid partId)
        {
            return await _context.Auctions.Include(a => a.Part).Include(a => a.Part.Category).OrderBy(a => a.Part.AveragePrice).
                FirstOrDefaultAsync(a => a.Status == AuctionStatuses.Active && a.Part.Id == partId);
        }

        public async Task<IReadOnlyList<Auction>> GetManyAsync()
        {
            return await _context.Auctions.Include(a => a.Part).Include(a => a.Part.Category).ToListAsync();
        }

        public async Task<List<Auction>> GetManyByPartAsync(Auction auction)
        {
            return await _context.Auctions.Include(a => a.Part).Include(a => a.Part.Category).
                Where(a => a.Status == AuctionStatuses.Active && a.Part.Id == auction.Part.Id && a.Id != auction.Id).ToListAsync();
        }

        public async Task<List<Auction>> GetManyBySeriesDifferentPartAsync(Auction auction)
        {
            return await _context.Auctions.Include(a => a.Part).Include(a => a.Part.Category).
                Where(a => a.Status == AuctionStatuses.Active && a.Part.Series.Id == auction.Part.Series.Id && a.Part.Id != auction.Part.Id).ToListAsync();
        }

        public async Task<List<Auction>> GetManyByCategoryDifferentSeriesAsync(Auction auction)
        {
            return await _context.Auctions.Include(a => a.Part).Include(a => a.Part.Category).
                Where(a => a.Status == AuctionStatuses.Active && a.Part.Category.Id == auction.Part.Category.Id &&
                a.Part.Series.Id != auction.Part.Series.Id && a.Part.Id != auction.Part.Id).ToListAsync();
        }

        public async Task<List<Auction>> GetManyByCategoryDifferentPartAsync(Auction auction)
        {
            return await _context.Auctions.Include(a => a.Part).Include(a => a.Part.Category).
                Where(a => a.Status == AuctionStatuses.Active && a.Part.Category.Id == auction.Part.Category.Id && a.Part.Id != auction.Part.Id).ToListAsync();
        }

        public async Task<IReadOnlyList<Auction>> GetManyWithPaginationAsync(int page, string categoryId, Guid? seriesId, Guid? partId, string sortType)
        {
            if(sortType == AuctionSortingTypes.TimeLeft)
            {
                return await _context.Auctions.Include(a => a.Part).Include(a => a.Part.Category).
                Where(a => (a.Status == AuctionStatuses.Active || a.Status == AuctionStatuses.ActiveNA) && (categoryId == null || a.Part.Category.Id == categoryId)
                && (seriesId == null || a.Part.Series.Id == seriesId) && (partId == null || a.Part.Id == partId)).
                OrderBy(a => a.EndDate).Skip((page - 1) * 5).Take(5).ToListAsync();
            }   
            else if(sortType == AuctionSortingTypes.CreationDate)
            {
                return await _context.Auctions.Include(a => a.Part).Include(a => a.Part.Category).
                Where(a => (a.Status == AuctionStatuses.Active || a.Status == AuctionStatuses.ActiveNA) && (categoryId == null || a.Part.Category.Id == categoryId)
                && (seriesId == null || a.Part.Series.Id == seriesId) && (partId == null || a.Part.Id == partId)).
                OrderByDescending(a => a.StartDate).Skip((page - 1) * 5).Take(5).ToListAsync();
            }

            return null;
        }

        public async Task<IReadOnlyList<Auction>> GetAllNewByUserAsync(string userId)
        {
            return await _context.Auctions.Include(a => a.Part).Include(a => a.Part.Category).
                Where(a => a.User.Id == userId && (a.Status == AuctionStatuses.New || a.Status == AuctionStatuses.NewNA)).
                OrderBy(a => a.StartDate).ToListAsync();
        }

        public async Task<IReadOnlyList<Auction>> GetAllActiveByUserAsync(string userId)
        {
            return await _context.Auctions.Include(a => a.Part).Include(a => a.Part.Category).
                Where(a => a.User.Id == userId && (a.Status == AuctionStatuses.Active || a.Status == AuctionStatuses.ActiveNA)).
                OrderBy(a => a.EndDate).ToListAsync();
        }

        public async Task<IReadOnlyList<Auction>> GetAllEndedByUserAsync(string userId)
        {
            return await _context.Auctions.Include(a => a.Part).Include(a => a.Part.Category).
                Where(a => a.User.Id == userId && (a.Status == AuctionStatuses.EndedWithoutBids || a.Status == AuctionStatuses.EndedWithBids || a.Status == AuctionStatuses.Paid)).
                OrderByDescending(a => a.EndDate).ToListAsync();
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

        public async Task<int> GetCountAsync(string categoryId, Guid? seriesId, Guid? partId)
        {
            return await _context.Auctions.Where(a => a.Status == AuctionStatuses.Active && (categoryId == null || a.Part.Category.Id == categoryId)
                && (seriesId == null || a.Part.Series.Id == seriesId) && (partId == null || a.Part.Id == partId)).CountAsync();
        }
    }
}
