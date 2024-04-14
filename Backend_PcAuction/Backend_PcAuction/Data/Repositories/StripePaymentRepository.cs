using Backend_PcAuction.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend_PcAuction.Data.Repositories
{
    public interface IStripePaymentRepository
    {
        Task CreateAsync(StripePayment stripePayment);
        Task<StripePayment?> GetLastAsync(Guid purchaseId);
    }

    public class StripePaymentRepository : IStripePaymentRepository
    {
        private readonly PcAuctionDbContext _context;

        public StripePaymentRepository(PcAuctionDbContext auctionDbContext)
        {
            _context = auctionDbContext;
        }

        public async Task CreateAsync(StripePayment stripePayment)
        {
            _context.StripePayment.Add(stripePayment);
            await _context.SaveChangesAsync();
        }

        public async Task<StripePayment?> GetLastAsync(Guid purchaseId)
        {
            return await _context.StripePayment.Include(s => s.Purchase).OrderByDescending(s => s.PaymentDate).FirstOrDefaultAsync(p => p.Purchase.Id == purchaseId);
        }
    }
}
