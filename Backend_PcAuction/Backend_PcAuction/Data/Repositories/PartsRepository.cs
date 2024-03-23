using Backend_PcAuction.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend_PcAuction.Data.Repositories
{
    public interface IPartsRepository
    {
        Task CreateAsync(Part part);
        Task DeleteAsync(Part part);
        Task<Part?> GetAsync(string categoryId, Guid partId);
        Task<IReadOnlyList<Part>> GetManyAsync(string categoryId);
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

        public async Task<Part?> GetAsync(string categoryId, Guid partId)
        {
            return await _context.Parts.Include(part => part.Series).FirstOrDefaultAsync(part => part.Id == partId && part.Category.Id == categoryId);
        }

        public async Task<IReadOnlyList<Part>> GetManyAsync(string categoryId)
        {
            return await _context.Parts.Include(part => part.Series).Where(part => part.Category.Id == categoryId).ToListAsync();
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
