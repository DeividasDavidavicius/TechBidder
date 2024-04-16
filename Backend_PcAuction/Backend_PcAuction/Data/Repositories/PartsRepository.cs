using Backend_PcAuction.Data.Entities;
using Backend_PcAuction.Utils;
using Microsoft.EntityFrameworkCore;

namespace Backend_PcAuction.Data.Repositories
{
    public interface IPartsRepository
    {
        Task CreateAsync(Part part);
        Task DeleteAsync(Part part);
        Task<Part?> GetAsync(string categoryId, Guid? partId);
        Task<Part?> GetForAnyCategoryAsync(Guid partId);
        Task<IReadOnlyList<Part>> GetManyAsync(string categoryId);
        Task<IReadOnlyList<Part>> GetManyTempAsync(string categoryId);
        Task<List<Part>> GetManyFromActiveAuctions(string categoryId);
        Task<Part?> GetFromActiveAuctions(string categoryId, Guid partId);

        Task UpdateAsync(Part part);
    }

    public class PartsRepository : IPartsRepository
    {
        private readonly PcAuctionDbContext _context;

        public PartsRepository(PcAuctionDbContext auctionDbContext)
        {
            _context = auctionDbContext;
        }

        public async Task CreateAsync(Part part)
        {
            _context.Parts.Add(part);
            await _context.SaveChangesAsync();
        }

        public async Task<Part?> GetAsync(string categoryId, Guid? partId)
        {
            return await _context.Parts.Include(p => p.Series).Include(p => p.Category).
                FirstOrDefaultAsync(part => part.Id == partId && part.Category.Id == categoryId);
        }

        public async Task<Part?> GetForAnyCategoryAsync(Guid partId)
        {
            return await _context.Parts.FirstOrDefaultAsync(part => part.Id == partId);
        }

        public async Task<IReadOnlyList<Part>> GetManyAsync(string categoryId)
        {
            return await _context.Parts.Include(part => part.Category).Include(part => part.Series).
                Where(part => part.Category.Id == categoryId && part.Type == PartTypes.Permanent).OrderBy(part => part.Name).ToListAsync();
        }

        public async Task<IReadOnlyList<Part>> GetManyTempAsync(string categoryId)
        {
            return await _context.Parts.Include(part => part.Series).Where(part => part.Category.Id == categoryId && part.Type == PartTypes.Temporary).
                OrderBy(part => part.Name).ToListAsync();
        }
        public async Task<Part?> GetFromActiveAuctions(string categoryId, Guid partId)
        {
            var auctionQuery = _context.Auctions
                .Where(a => a.Status == AuctionStatuses.Active && a.Part.Id == partId && a.Part.Category.Id == categoryId && a.Part.AveragePrice > 0).
                Include(a => a.Part.Category)
                .Select(a => new
                {
                    Auction = a,
                    HighestBidAmount = _context.Bids.Where(b => b.Auction.Id == a.Id).OrderByDescending(b => b.Amount).Select(b => b.Amount).FirstOrDefault()
                });

            var filteredParts = await auctionQuery
                .Where(a => a.HighestBidAmount < a.Auction.Part.AveragePrice)
                .Select(a => a.Auction.Part)
                .FirstOrDefaultAsync();
            return filteredParts;
        }

        public async Task<List<Part>> GetManyFromActiveAuctions(string categoryId)
        {
            var auctionQuery = _context.Auctions
            .Where(a => a.Status == AuctionStatuses.Active && a.Part.Category.Id == categoryId && a.Part.AveragePrice > 0).Include(a => a.Part.Category)
                .Select(a => new
                {
                    Auction = a,
                    HighestBidAmount = _context.Bids.Where(b => b.Auction.Id == a.Id).OrderByDescending(b => b.Amount).Select(b => b.Amount).FirstOrDefault()
                });

            var filteredParts = await auctionQuery
                .Where(a => a.HighestBidAmount < a.Auction.Part.AveragePrice)
                .Select(a => a.Auction.Part)
                .Distinct().ToListAsync();
            return filteredParts;
        }

        public async Task UpdateAsync(Part part)
        {
            _context.Parts.Update(part);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Part part)
        {
            _context.Parts.Remove(part);
            await _context.SaveChangesAsync();
        }
    }
}
