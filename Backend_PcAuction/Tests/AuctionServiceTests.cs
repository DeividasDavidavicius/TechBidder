using Backend_PcAuction.Data.Entities;
using Backend_PcAuction.Data.Repositories;
using Backend_PcAuction.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    public class AuctionServiceTests
    {
        private readonly Mock<IBidsRepository> _bidsRepositoryMock;
        private readonly Mock<IAuctionsRepository> _auctionsRepositoryMock;
        private readonly AuctionsService _auctionService;

        public AuctionServiceTests()
        {
            _bidsRepositoryMock = new Mock<IBidsRepository>();
            _auctionsRepositoryMock = new Mock<IAuctionsRepository>();
            _auctionService = new AuctionsService(_bidsRepositoryMock.Object, _auctionsRepositoryMock.Object);
        }

        [Fact]
        public async Task GenerateAuctionRecommendations_ReturnsExpectedRecommendations()
        {
            var auction = new Auction { Part = new Part { Series = new Series { } } };

            var samePartAuctions = new List<Auction> { new Auction { Part = new Part { Series = new Series { } } } };
            var sameSeriesAuctions = new List<Auction> { new Auction { Part = new Part { Series = new Series { } } } };
            var sameCategoryAuctions = new List<Auction> { new Auction { Part = new Part { Series = new Series { } } } };

            _auctionsRepositoryMock
                .Setup(repo => repo.GetManyByPartAsync(auction))
                .ReturnsAsync(samePartAuctions);

            _auctionsRepositoryMock
                .Setup(repo => repo.GetManyBySeriesDifferentPartAsync(auction))
                .ReturnsAsync(sameSeriesAuctions);

            _auctionsRepositoryMock
                .Setup(repo => repo.GetManyByCategoryDifferentSeriesAsync(auction))
                .ReturnsAsync(sameCategoryAuctions);

            _auctionsRepositoryMock
                .Setup(repo => repo.GetManyByCategoryDifferentPartAsync(auction))
                .ReturnsAsync(sameCategoryAuctions);

            var recommendations = await _auctionService.GenerateAuctionRecommendations(auction);

            Assert.NotNull(recommendations);
            Assert.Equal(3, recommendations.Count);
        }
    }
}
