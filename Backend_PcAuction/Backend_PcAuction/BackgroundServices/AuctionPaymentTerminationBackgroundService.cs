using Backend_PcAuction.Data.Entities;
using Backend_PcAuction.Data;
using Backend_PcAuction.Utils;
using Microsoft.EntityFrameworkCore;

namespace Backend_PcAuction.BackgroundServices
{
    public class AuctionPaymentTerminationBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly TimeSpan _interval;

        public AuctionPaymentTerminationBackgroundService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            _interval = TimeSpan.FromMinutes(5);
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await CheckAuctionPaymentStatus();
                await Task.Delay(_interval, token);
            }
        }

        private async Task CheckAuctionPaymentStatus()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<PcAuctionDbContext>();

                var auctions = await dbContext.Auctions.Where(a => a.Status == AuctionStatuses.EndedWithBids).ToListAsync();

                var currentTime = DateTime.UtcNow;

                foreach (var auction in auctions)
                {
                    if (auction.EndDate.AddDays(14) <= currentTime)
                    {
                        auction.Status = AuctionStatuses.PaymentNotReceived;
                        dbContext.Auctions.Update(auction);

                        var purchase = await dbContext.Purchases.FirstOrDefaultAsync(p => p.Auction.Id == auction.Id && p.Status == PurchaseStatuses.PendingPayment);

                        if(purchase != null)
                        {
                            purchase.Status = PurchaseStatuses.Terminated;
                            dbContext.Purchases.Update(purchase);
                        }

                        var log = new Log
                        {
                            Message = String.Format("Updated auction status to {2}: Time: {0}, auction: {1}", DateTime.UtcNow, auction.Name, auction.Status)
                        };
                        dbContext.Logs.Add(log);
                    }

                    await dbContext.SaveChangesAsync();
                }
            }
        }
    }
}
