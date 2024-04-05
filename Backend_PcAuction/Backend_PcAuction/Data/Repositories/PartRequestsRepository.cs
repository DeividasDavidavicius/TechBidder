using Backend_PcAuction.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend_PcAuction.Data.Repositories
{
    public interface IPartRequestsRepository
    {
        Task CreateAsync(PartRequest partRequest);
        Task DeleteAsync(PartRequest partRequest);
        Task<PartRequest?> GetAsync(Guid? partRequestId);
        Task<IReadOnlyList<PartRequest>> GetManyAsync();
    }

    public class PartRequestsRepository : IPartRequestsRepository
    {
        private readonly PcAuctionDbContext _context;

        public PartRequestsRepository(PcAuctionDbContext auctionDbContext)
        {
            _context = auctionDbContext;
        }

        public async Task CreateAsync(PartRequest partRequest)
        {
            _context.PartRequests.Add(partRequest);
            await _context.SaveChangesAsync();
        }

        public async Task<PartRequest?> GetAsync(Guid? partRequestId)
        {
            return await _context.PartRequests.Include(r => r.Auction).Include(r => r.Part).Include(r => r.Part.Category).FirstOrDefaultAsync(partRequest => partRequest.Id == partRequestId);
        }

        public async Task<IReadOnlyList<PartRequest>> GetManyAsync()
        {
            return await _context.PartRequests.Include(r => r.Auction).Include(r => r.Part).Include(r => r.Part.Category).ToListAsync();
        }

        public async Task DeleteAsync(PartRequest partRequest)
        {
            _context.PartRequests.Remove(partRequest);
            await _context.SaveChangesAsync();
        }
    }
}
