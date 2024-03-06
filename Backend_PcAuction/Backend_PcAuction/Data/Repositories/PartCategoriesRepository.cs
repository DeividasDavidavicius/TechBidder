using Backend_PcAuction.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend_PcAuction.Data.Repositories
{
    public interface IPartCategoriesRepository
    {
        Task CreateAsync(PartCategory category);
        Task DeleteAsync(PartCategory category);
        Task<PartCategory?> GetAsync(string categoryId);
        Task<IReadOnlyList<PartCategory>> GetManyAsync();
    }

    public class PartCategoriesRepository : IPartCategoriesRepository
    {
        private readonly PcAuctionDbContext _context;

        public PartCategoriesRepository(PcAuctionDbContext auctionDbContext)
        {
            _context = auctionDbContext;
        }

        public async Task CreateAsync(PartCategory category)
        {
            _context.PartCategories.Add(category);
            await _context.SaveChangesAsync();
        }

        public async Task<PartCategory?> GetAsync(string categoryId)
        {
            return await _context.PartCategories.FirstOrDefaultAsync(category => category.Id == categoryId);
        }

        public async Task<IReadOnlyList<PartCategory>> GetManyAsync()
        {
            return await _context.PartCategories.ToListAsync();
        }

        public async Task DeleteAsync(PartCategory category)
        {
            _context.PartCategories.Remove(category);
            await _context.SaveChangesAsync();
        }
    }
}
