using Backend_PcAuction.Data.Entities;
using Backend_PcAuction.Data.Repositories;
using Backend_PcAuction.Utils;

namespace Backend_PcAuction.Services
{
    public interface IPartPricesService
    {
        Task<double> GetPriceAverageAsync(Guid partId);
    }

    public class PartPricesService : IPartPricesService
    {
        private readonly IPartsPricesRepository _partsPricesRepository;

        public PartPricesService(IPartsPricesRepository partsPricesRepository)
        {
            _partsPricesRepository = partsPricesRepository;
        }

        public async Task<double> GetPriceAverageAsync(Guid partId)
        {
            var ebayPriceAvg = await _partsPricesRepository.GetAsync(partId, PartPriceTypes.EbayAverage);

            var salesOnPartAuction = await _partsPricesRepository.GetManyAsync(partId, PartPriceTypes.PartAuctionSell);

            var sumOfSales = 0.0;
            double factors = 0;
            double returnValue = -1;

            foreach (var sale in salesOnPartAuction)
            {
                sumOfSales += sale.Price;
            }

            if(ebayPriceAvg != null)
            {
                if(ebayPriceAvg.Price == -10)
                {
                    ebayPriceAvg = null;
                    returnValue = 0;
                }
                else
                {
                    factors++;
                }
            }

            if(salesOnPartAuction.Count > 0)
            {
                factors++;
            }

            if(factors == 0)
            {
                return returnValue;
            }

            var ebayAvgAmount = ebayPriceAvg != null ? ebayPriceAvg.Price : 0;
            var partAuctionAvg = salesOnPartAuction.Count > 0 ? sumOfSales / salesOnPartAuction.Count : 0;
            var totalAverage = (ebayAvgAmount + partAuctionAvg) / factors;

            return Math.Truncate(totalAverage);
        }
    }
}
