using Backend_PcAuction.Data.Dtos;
using Backend_PcAuction.Data.Entities;
using Backend_PcAuction.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Backend_PcAuction.Services
{
    public interface IAuctionService
    {
        Task<List<Auction>> GenerateAuctionRecommendations(Auction auction);
    }

    public class AuctionService : IAuctionService
    {

        private readonly IBidsRepository _bidsRepository;
        private readonly IAuctionsRepository _auctionsRepository;
        private readonly IPartPricesService _partPricesService;

        public AuctionService(IBidsRepository bidsRepository, IAuctionsRepository auctionsRepository, IPartPricesService partPricesService)
        {
            _bidsRepository = bidsRepository;
            _auctionsRepository = auctionsRepository;
            _partPricesService = partPricesService;
        }

        public async Task<List<Auction>> GenerateAuctionRecommendations(Auction auction)
        {
            List<Auction> recommendations = new List<Auction>();
            Random random = new Random();


            var samePartAuctions = await _auctionsRepository.GetManyByPartAsync(auction);

            var samePartAuctionsFiltered = samePartAuctions.ToList();

            foreach(var recAuction in samePartAuctions)
            {
                var highestBid = await _bidsRepository.GetLastAsync(recAuction.Id);

                if(highestBid != null && highestBid.Amount > recAuction.Part.AveragePrice && recAuction.Part.AveragePrice > 0)
                {
                    samePartAuctionsFiltered.Remove(recAuction);
                }
            }

            for (int i = 0; i < 2 && samePartAuctionsFiltered.Count > 0; i++)
            {
                int randomIndex = random.Next(0, samePartAuctionsFiltered.Count);
                recommendations.Add(samePartAuctionsFiltered[randomIndex]);
                samePartAuctionsFiltered.RemoveAt(randomIndex);
            }

            var sameSeriesAuctions = (auction.Part.Series != null ? await _auctionsRepository.GetManyBySeriesDifferentPartAsync(auction) : new List<Auction>());
            var sameSeriesAuctionsFiltered = sameSeriesAuctions.ToList();

            foreach (var recAuction in sameSeriesAuctions)
            {
                var highestBid = await _bidsRepository.GetLastAsync(recAuction.Id);

                if (highestBid != null && highestBid.Amount > recAuction.Part.AveragePrice && recAuction.Part.AveragePrice > 0)
                {
                    sameSeriesAuctionsFiltered.Remove(recAuction);
                }
            }


            for (int i = 0; i < 2 && sameSeriesAuctionsFiltered.Count > 0; i++)
            {
                int randomIndex = random.Next(0, sameSeriesAuctionsFiltered.Count);
                recommendations.Add(sameSeriesAuctionsFiltered[randomIndex]);
                sameSeriesAuctionsFiltered.RemoveAt(randomIndex);
            }

            var sameCategoryAuctions = auction.Part.Series != null ?  await _auctionsRepository.GetManyByCategoryDifferentSeriesAsync(auction) : await _auctionsRepository.GetManyByCategoryDifferentPartAsync(auction);
            var sameCategoryAuctionsFiltered = sameCategoryAuctions.ToList();

            foreach (var recAuction in sameCategoryAuctions)
            {
                var highestBid = await _bidsRepository.GetLastAsync(recAuction.Id);

                if (highestBid != null && highestBid.Amount > recAuction.Part.AveragePrice && recAuction.Part.AveragePrice > 0)
                {
                    sameCategoryAuctionsFiltered.Remove(recAuction);
                }
            }

            var allRemainingAuctions = new List<Auction>();

            allRemainingAuctions.AddRange(samePartAuctionsFiltered);
            allRemainingAuctions.AddRange(sameSeriesAuctionsFiltered);
            allRemainingAuctions.AddRange(sameCategoryAuctionsFiltered);

            while (recommendations.Count < 7 && allRemainingAuctions.Count > 0)
            {
                int randomIndex = random.Next(0, allRemainingAuctions.Count);
                recommendations.Add(allRemainingAuctions[randomIndex]);
                allRemainingAuctions.RemoveAt(randomIndex);
            }

            return recommendations;
        }
    }
}
