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
            var highestBid = auction.HighestBid;

            if(highestBid == -1)
            {
                return null;
            }

            var partAveragePrice = await _partPricesService.GetPriceAverageAsync(auction.Part.Id);

            var samePartAuctions = await _auctionsRepository.GetManyByPartAsync(auction.Part.Id);
            var samePartAuctionsFiltered = samePartAuctions.Where(a => a.HighestBid < highestBid && a.HighestBid < partAveragePrice).ToList();

            return null;
        }
    }
}
