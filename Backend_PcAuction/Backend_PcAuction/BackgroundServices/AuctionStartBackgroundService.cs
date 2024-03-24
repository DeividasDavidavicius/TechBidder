﻿using Backend_PcAuction.Data;
using Backend_PcAuction.Data.Entities;
using Backend_PcAuction.Data.Repositories;
using Backend_PcAuction.Utils;
using Microsoft.EntityFrameworkCore;

namespace Backend_PcAuction.BackgroundServices
{
    public class AuctionStartBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly TimeSpan _interval;

        public AuctionStartBackgroundService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            _interval = TimeSpan.FromMinutes(1);
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            while(!token.IsCancellationRequested)
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

                var auctions = await dbContext.Auctions.Where(a => a.Status == AuctionStatuses.New).ToListAsync();

                var currentTime = DateTime.UtcNow;

                foreach (var auction in auctions)
                {
                    if (auction.StartDate <= currentTime)
                    {
                        auction.Status = AuctionStatuses.Active;
                        dbContext.Auctions.Update(auction);

                        var log = new Log
                        {
                            Message = String.Format("Updated: Time: {0}, auction: {1}", DateTime.UtcNow, auction.Name)
                        };
                        dbContext.Logs.Add(log);
                    }
                    else
                    {
                        var log = new Log
                        {
                            Message = String.Format("Not updated: Time: {0}, auction: {1}", DateTime.UtcNow, auction.Name)
                        };
                        dbContext.Logs.Add(log);
                    }
                    await dbContext.SaveChangesAsync();
                }
            }
        }
    }
}
