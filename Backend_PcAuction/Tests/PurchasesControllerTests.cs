using Backend_PcAuction.Controllers;
using Backend_PcAuction.Data.Dtos;
using Backend_PcAuction.Data.Entities;
using Backend_PcAuction.Data.Repositories;
using Backend_PcAuction.Services;
using Backend_PcAuction.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    public class PurchasesControllerTests
    {
        private readonly Mock<IPurchaseRepository> _purchaseRepositoryMock;
        private readonly Mock<IAuctionsRepository> _auctionsRepositoryMock;
        private readonly Mock<IStripePaymentRepository> _stripePaymentRepositoryMock;
        private readonly Mock<IPartsRepository> _partsRepositoryMock;
        private readonly Mock<IPartsPricesRepository> _partsPricesRepositoryMock;
        private readonly Mock<IPartPricesService> _partPricesServiceMock;
        private readonly IOptions<StripeSettings> _stripeSettingsOptionsMock;

        private readonly PurchasesController _controller;

        public PurchasesControllerTests()
        {
            _purchaseRepositoryMock = new Mock<IPurchaseRepository>();
            _auctionsRepositoryMock = new Mock<IAuctionsRepository>();
            _stripePaymentRepositoryMock = new Mock<IStripePaymentRepository>();
            _partsRepositoryMock = new Mock<IPartsRepository>();
            _partsPricesRepositoryMock = new Mock<IPartsPricesRepository>();
            _partPricesServiceMock = new Mock<IPartPricesService>();
            var stripeSettings = new StripeSettings();
            _stripeSettingsOptionsMock = Options.Create(stripeSettings);


            _controller = new PurchasesController(
                _purchaseRepositoryMock.Object,
                _auctionsRepositoryMock.Object,
                _stripePaymentRepositoryMock.Object,
                _partsRepositoryMock.Object,
                _partsPricesRepositoryMock.Object,
                _partPricesServiceMock.Object,
                _stripeSettingsOptionsMock
            );
        }

        [Fact]
        public async Task Get_ReturnsNotFound_WhenAuctionNotFound()
        {
            var auctionId = Guid.NewGuid();
            _auctionsRepositoryMock.Setup(repo => repo.GetAsync(auctionId)).ReturnsAsync((Auction)null);

            var result = await _controller.Get(auctionId);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Get_ReturnsNotFound_WhenPurchaseNotFound()
        {
            var auctionId = Guid.NewGuid();
            var auction = new Auction { Id = auctionId };
            _auctionsRepositoryMock.Setup(repo => repo.GetAsync(auctionId)).ReturnsAsync(auction);
            _purchaseRepositoryMock.Setup(repo => repo.GetLastAsync(auctionId)).ReturnsAsync((Purchase)null);

            var result = await _controller.Get(auctionId);

            Assert.IsType<NotFoundResult>(result.Result);
        }




    }
}
