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
            var samePartAuctionsFiltered = samePartAuctions.Where(a => a.HighestBid < 1000).ToList(); // TODO replace 1000 with average price

            for (int i = 0; i < 2 && samePartAuctionsFiltered.Count > 0; i++)
            {
                int randomIndex = random.Next(0, samePartAuctionsFiltered.Count);
                recommendations.Add(samePartAuctionsFiltered[randomIndex]);
                samePartAuctionsFiltered.RemoveAt(randomIndex);
            }

            var sameSeriesAuctions = (auction.Part.Series != null ? await _auctionsRepository.GetManyBySeriesDifferentPartAsync(auction) : new List<Auction>());
            var sameSeriesAuctionsFiltered = sameSeriesAuctions.Where(a => a.HighestBid < 1000).ToList(); // TODO replace 1000 with average price

            
            for (int i = 0; i < 2 && sameSeriesAuctionsFiltered.Count > 0; i++)
            {
                int randomIndex = random.Next(0, sameSeriesAuctionsFiltered.Count);
                recommendations.Add(sameSeriesAuctionsFiltered[randomIndex]);
                sameSeriesAuctionsFiltered.RemoveAt(randomIndex);
            }

            var sameCategoryAuctions = auction.Part.Series != null ?  await _auctionsRepository.GetManyByCategoryDifferentSeriesAsync(auction) : await _auctionsRepository.GetManyByCategoryDifferentPartAsync(auction);
            var sameCategoryAuctionsFiltered = sameCategoryAuctions.Where(a => a.HighestBid < 1000).ToList(); // TODO replace 1000 with average price

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
