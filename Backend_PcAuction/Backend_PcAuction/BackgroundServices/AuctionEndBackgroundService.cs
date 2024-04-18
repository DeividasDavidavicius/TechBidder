using Backend_PcAuction.Data.Entities;
using Backend_PcAuction.Data;
using Backend_PcAuction.Utils;
using Microsoft.EntityFrameworkCore;

namespace Backend_PcAuction.BackgroundServices
{
    public class AuctionEndBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly TimeSpan _interval;

        public AuctionEndBackgroundService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            _interval = TimeSpan.FromSeconds(5);
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await CheckAuctionStartTime();
                await Task.Delay(_interval, token);
            }
        }

        private async Task CheckAuctionStartTime()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<PcAuctionDbContext>();

                var currentTime = DateTime.UtcNow;

                var auctions = await dbContext.Auctions.Where(a => (a.Status == AuctionStatuses.Active || a.Status == AuctionStatuses.ActiveNA) && a.EndDate <= currentTime).ToListAsync();


                foreach (var auction in auctions)
                {
                    var bids = await dbContext.Bids.Where(b => b.Auction.Id == auction.Id).ToListAsync();

                    auction.Status = bids.Count > 0 ? AuctionStatuses.EndedWithBids : AuctionStatuses.EndedWithoutBids;
                    dbContext.Auctions.Update(auction);

                    var log = new Log
                    {
                        Message = String.Format("Updated auction status to {2}: Time: {0}, auction: {1}", DateTime.UtcNow, auction.Name, auction.Status)
                    };

                    dbContext.Logs.Add(log);
                    await dbContext.SaveChangesAsync();

                    if(bids.Count > 0)
                    {
                        var highestBid = await dbContext.Bids.OrderByDescending(b => b.Amount).Include(b => b.User).FirstOrDefaultAsync(b => b.Auction.Id == auction.Id);

                        var purchase = new Purchase
                        {
                            Amount = highestBid.Amount,
                            Status = PurchaseStatuses.PendingPayment,
                            AuctionWinDate = DateTime.UtcNow,
                            Auction = auction,
                            BuyerId = highestBid.UserId,
                            Buyer = highestBid.User
                        };

                        dbContext.Purchases.Add(purchase);
                        await dbContext.SaveChangesAsync();
                    }
                }
            }
        }
    }
}
