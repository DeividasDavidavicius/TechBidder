using Backend_PcAuction.Data.Entities;
using Backend_PcAuction.Data.Repositories;
using Backend_PcAuction.Services;
using Backend_PcAuction.Utils;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    public class PartPriceServiceTests
    {
        private readonly Mock<IPartsPricesRepository> _partPricesRepositoryMock;
        private readonly PartPricesService _partPricesService;

        public PartPriceServiceTests()
        {
            _partPricesRepositoryMock = new Mock<IPartsPricesRepository>();
            _partPricesService = new PartPricesService(_partPricesRepositoryMock.Object);
        }

        [Fact]
        public async Task GetPriceAverageAsync_NoSalesAndNoEbayAverage_ReturnsNegativeOne()
        {
            var partId = Guid.NewGuid();
            _partPricesRepositoryMock.Setup(repo => repo.GetAsync(partId, PartPriceTypes.EbayAverage)).ReturnsAsync((PartPrice)null);
            _partPricesRepositoryMock.Setup(repo => repo.GetManyAsync(partId, PartPriceTypes.PartAuctionSell)).ReturnsAsync(new List<PartPrice>());

            var result = await _partPricesService.GetPriceAverageAsync(partId);

            Assert.Equal(-1, result);
        }

        [Fact]
        public async Task GetPriceAverageAsync_NoSalesButWithEbayAverage_ReturnsEbayAverage()
        {
            var partId = Guid.NewGuid();
            var ebayAverage = new PartPrice { Price = 200 };
            _partPricesRepositoryMock.Setup(repo => repo.GetAsync(partId, PartPriceTypes.EbayAverage)).ReturnsAsync(ebayAverage);
            _partPricesRepositoryMock.Setup(repo => repo.GetManyAsync(partId, PartPriceTypes.PartAuctionSell)).ReturnsAsync(new List<PartPrice>());

            var result = await _partPricesService.GetPriceAverageAsync(partId);

            Assert.Equal(200.0, result);
        }

        [Fact]
        public async Task GetPriceAverageAsync_NoEbayAverageButWithSales_ReturnsAverageOfSales()
        {
            var partId = Guid.NewGuid();
            var sales = new List<PartPrice>
            {
                new PartPrice { Price = 150 },
                new PartPrice { Price = 200 },
                new PartPrice { Price = 250 }
            };
            _partPricesRepositoryMock.Setup(repo => repo.GetAsync(partId, PartPriceTypes.EbayAverage)).ReturnsAsync((PartPrice)null);
            _partPricesRepositoryMock.Setup(repo => repo.GetManyAsync(partId, PartPriceTypes.PartAuctionSell)).ReturnsAsync(sales);

            var result = await _partPricesService.GetPriceAverageAsync(partId);

            Assert.Equal(200.0, result);
        }

        [Fact]
        public async Task GetPriceAverageAsync_EbayAverageIsNegativeTen_ReturnsZero()
        {
            var partId = Guid.NewGuid();
            var ebayAverage = new PartPrice { Price = -10.0 };
            _partPricesRepositoryMock.Setup(repo => repo.GetAsync(partId, PartPriceTypes.EbayAverage)).ReturnsAsync(ebayAverage);
            _partPricesRepositoryMock.Setup(repo => repo.GetManyAsync(partId, PartPriceTypes.PartAuctionSell)).ReturnsAsync(new List<PartPrice>());

            var result = await _partPricesService.GetPriceAverageAsync(partId);

            Assert.Equal(0, result);
        }
    }
}
