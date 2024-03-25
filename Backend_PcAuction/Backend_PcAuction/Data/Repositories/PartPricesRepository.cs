using Backend_PcAuction.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend_PcAuction.Data.Repositories
{
    public class PartsPricesRepository
    {
        private readonly PcAuctionDbContext _context;

        public PartsPricesRepository(PcAuctionDbContext auctionDbContext)
        {
            _context = auctionDbContext;
        }

        public async Task CreateAsync(PartPrice partPrice)
        {
            _context.PartPrices.Add(partPrice);
            await _context.SaveChangesAsync();
        }

        public async Task<PartPrice?> GetAsync(Guid partId, string type)
        {
            var sortedSequence = _context.PartPrices.OrderByDescending(partPrice => partPrice.PriceCheckDate);
            return await sortedSequence.FirstOrDefaultAsync(partPrice => partPrice.Part.Id == partId && partPrice.Type == type);
        }

        public async Task<IReadOnlyList<PartPrice>> GetManyAsync(Guid partId, string type)
        {
            return await _context.PartPrices.Where(partPrice => partPrice.Part.Id == partId && partPrice.Type == type).ToListAsync();
        }

        public async Task DeleteAsync(PartPrice partPrice)
        {
            _context.PartPrices.Remove(partPrice);
            await _context.SaveChangesAsync();
        }
    }
}
