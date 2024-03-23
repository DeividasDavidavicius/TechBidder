using Backend_PcAuction.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend_PcAuction.Data.Repositories
{
    public interface ISeriesRepository
    {
        Task CreateAsync(Series series);
        Task DeleteAsync(Series series);
        Task<Series?> GetAsync(string categoryId, Guid? seriesId);
        Task<IReadOnlyList<Series>> GetManyAsync(string categoryId);
        Task UpdateAsync(Series series);
    }

    public class SeriesRepository : ISeriesRepository
    {
        private readonly PcAuctionDbContext _context;

        public SeriesRepository(PcAuctionDbContext auctionDbContext)
        {
            _context = auctionDbContext;
        }

        public async Task CreateAsync(Series series)
        {
            _context.Series.Add(series);
            await _context.SaveChangesAsync();
        }

        public async Task<Series?> GetAsync(string categoryId, Guid? seriesId)
        {
            return await _context.Series.FirstOrDefaultAsync(series => series.Id == seriesId && series.PartCategory.Id == categoryId);
        }

        public async Task<IReadOnlyList<Series>> GetManyAsync(string categoryId)
        {
            return await _context.Series.Where(series => series.PartCategory.Id == categoryId).ToListAsync();
        }

        public async Task UpdateAsync(Series series)
        {
            _context.Series.Update(series);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Series series)
        {
            _context.Series.Remove(series);
            await _context.SaveChangesAsync();
        }
    }
}
