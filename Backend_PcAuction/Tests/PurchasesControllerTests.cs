using Backend_PcAuction.Controllers;
using Backend_PcAuction.Data.Dtos;
using Backend_PcAuction.Data.Entities;
using Backend_PcAuction.Data.Repositories;
using Backend_PcAuction.Services;
using Backend_PcAuction.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
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
        private readonly Mock<IStripeService> _stripeServiceMock;

        private readonly PurchasesController _controller;

        public PurchasesControllerTests()
        {
            _purchaseRepositoryMock = new Mock<IPurchaseRepository>();
            _auctionsRepositoryMock = new Mock<IAuctionsRepository>();
            _stripePaymentRepositoryMock = new Mock<IStripePaymentRepository>();
            _partsRepositoryMock = new Mock<IPartsRepository>();
            _partsPricesRepositoryMock = new Mock<IPartsPricesRepository>();
            _partPricesServiceMock = new Mock<IPartPricesService>();
            _stripeServiceMock = new Mock<IStripeService>();


            _controller = new PurchasesController(
                _purchaseRepositoryMock.Object,
                _auctionsRepositoryMock.Object,
                _stripePaymentRepositoryMock.Object,
                _partsRepositoryMock.Object,
                _partsPricesRepositoryMock.Object,
                _partPricesServiceMock.Object,
                _stripeServiceMock.Object
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

        [Fact]
        public async Task Get_ValidAuctionId_ReturnsPurchaseDto()
        {
            var auctionId = Guid.NewGuid();
            var purchaseId = Guid.NewGuid();
            var buyerId = "Id";

            var auction = new Auction { Id = auctionId };
            var purchase = new Purchase { Id = purchaseId, Amount = 100, Status = PurchaseStatuses.Paid, AuctionWinDate = DateTime.UtcNow, Buyer = new User { Id = buyerId }, Auction = auction };

            _auctionsRepositoryMock.Setup(repo => repo.GetAsync(auctionId)).ReturnsAsync(auction);
            _purchaseRepositoryMock.Setup(repo => repo.GetLastAsync(auctionId)).ReturnsAsync(purchase);

            var result = await _controller.Get(auctionId);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var purchaseDto = Assert.IsType<PurchaseDto>(okResult.Value);

            Assert.Equal(purchase.Id, purchaseDto.Id);
            Assert.Equal(purchase.Amount, purchaseDto.Amount);
            Assert.Equal(purchase.Status, purchaseDto.Status);
            Assert.Equal(purchase.AuctionWinDate, purchaseDto.AuctionWinDate);
            Assert.Equal(purchase.Buyer.Id, purchaseDto.BuyerId);
            Assert.Equal(purchase.Auction.Id, purchaseDto.AuctionId);
        }

        [Fact]
        public async Task Update_ValidInput_ReturnsOk()
        {
            var auctionId = Guid.NewGuid();
            var purchaseId = Guid.NewGuid();
            var buyerId = "BuyerId";

            var auction = new Auction { Id = auctionId, Part = new Part { Id = Guid.NewGuid() } };
            var purchase = new Purchase { Id = purchaseId, Amount = 100, Status = PurchaseStatuses.PendingPayment, Buyer = new User { Id = buyerId }, Auction = auction };
            var stripePayment = new StripePayment { Id = "Id" };

            _auctionsRepositoryMock.Setup(repo => repo.GetAsync(auctionId)).ReturnsAsync(auction);
            _purchaseRepositoryMock.Setup(repo => repo.GetLastAsync(auctionId)).ReturnsAsync(purchase);
            _stripeServiceMock.Setup(service => service.GetPurchaseStatus(It.IsAny<string>())).ReturnsAsync("paid");
            _stripePaymentRepositoryMock.Setup(repo => repo.GetLastAsync(purchaseId)).ReturnsAsync(stripePayment);

            var result = await _controller.Update(auctionId);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var purchaseDto = Assert.IsType<PurchaseDto>(okResult.Value);

            Assert.Equal(purchase.Id, purchaseDto.Id);
            Assert.Equal(purchase.Amount, purchaseDto.Amount);
            Assert.Equal(PurchaseStatuses.Paid, purchaseDto.Status);
            Assert.Equal(purchase.AuctionWinDate, purchaseDto.AuctionWinDate);
            Assert.Equal(purchase.Buyer.Id, purchaseDto.BuyerId);
            Assert.Equal(purchase.Auction.Id, purchaseDto.AuctionId);
        }

        [Fact]
        public async Task Update_AuctionNotFound_ReturnsNotFound()
        {
            var auctionId = Guid.NewGuid();

            _auctionsRepositoryMock.Setup(repo => repo.GetAsync(auctionId)).ReturnsAsync((Auction)null);

            var result = await _controller.Update(auctionId);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Update_PurchaseNotFound_ReturnsNotFound()
        {
            var auctionId = Guid.NewGuid();
            var purchaseId = Guid.NewGuid();

            var auction = new Auction { Id = auctionId };

            _auctionsRepositoryMock.Setup(repo => repo.GetAsync(auctionId)).ReturnsAsync(auction);
            _purchaseRepositoryMock.Setup(repo => repo.GetLastAsync(auctionId)).ReturnsAsync((Purchase)null);

            var result = await _controller.Update(auctionId);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Update_PurchaseStatusNotPendingPayment_ReturnsForbid()
        {
            var auctionId = Guid.NewGuid();
            var purchaseId = Guid.NewGuid();

            var auction = new Auction { Id = auctionId };
            var purchase = new Purchase { Id = purchaseId, Status = PurchaseStatuses.Paid };

            _auctionsRepositoryMock.Setup(repo => repo.GetAsync(auctionId)).ReturnsAsync(auction);
            _purchaseRepositoryMock.Setup(repo => repo.GetLastAsync(auctionId)).ReturnsAsync(purchase);

            var result = await _controller.Update(auctionId);

            Assert.IsType<ForbidResult>(result.Result);
        }

        [Fact]
        public async Task Update_PaymentStatusNotPaid_ReturnsForbid()
        {
            var auctionId = Guid.NewGuid();
            var purchaseId = Guid.NewGuid();

            var auction = new Auction { Id = auctionId };
            var purchase = new Purchase { Id = purchaseId, Status = PurchaseStatuses.PendingPayment };
            var stripePayment = new StripePayment { Id = "Id" };

            _auctionsRepositoryMock.Setup(repo => repo.GetAsync(auctionId)).ReturnsAsync(auction);
            _purchaseRepositoryMock.Setup(repo => repo.GetLastAsync(auctionId)).ReturnsAsync(purchase);
            _stripeServiceMock.Setup(service => service.GetPurchaseStatus(It.IsAny<string>())).ReturnsAsync("failed");
            _stripePaymentRepositoryMock.Setup(repo => repo.GetLastAsync(purchaseId)).ReturnsAsync(stripePayment);

            var result = await _controller.Update(auctionId);

            Assert.IsType<ForbidResult>(result.Result);
        }

        [Fact]
        public async Task PostStripe_ValidInput_ReturnsOk()
        {
            var auctionId = Guid.NewGuid();
            var purchaseId = Guid.NewGuid();
            var buyerId = Guid.NewGuid().ToString();

            var auction = new Auction { Id = auctionId };
            var purchase = new Purchase { Id = purchaseId, BuyerId = buyerId };
            var purchaseStripeDto = new PurchaseStripeDto("Id", "Url");

            _auctionsRepositoryMock.Setup(repo => repo.GetAsync(auctionId)).ReturnsAsync(auction);
            _purchaseRepositoryMock.Setup(repo => repo.GetLastAsync(auctionId)).ReturnsAsync(purchase);
            _stripeServiceMock.Setup(service => service.CreateStripeSession(It.IsAny<Auction>(), It.IsAny<Purchase>())).ReturnsAsync(purchaseStripeDto);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, buyerId)
            };
            var identity = new ClaimsIdentity(claims);
            var userPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };

            var result = await _controller.PostStripe(auctionId);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(purchaseStripeDto, okResult.Value);
        }

        [Fact]
        public async Task PostStripe_AuctionNotFound_ReturnsNotFound()
        {
            var auctionId = Guid.NewGuid();

            _auctionsRepositoryMock.Setup(repo => repo.GetAsync(auctionId)).ReturnsAsync((Auction)null);

            var result = await _controller.PostStripe(auctionId);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task PostStripe_PurchaseNotFound_ReturnsNotFound()
        {
            var auctionId = Guid.NewGuid();

            _auctionsRepositoryMock.Setup(repo => repo.GetAsync(auctionId)).ReturnsAsync(new Auction());
            _purchaseRepositoryMock.Setup(repo => repo.GetLastAsync(auctionId)).ReturnsAsync((Purchase)null);

            var result = await _controller.PostStripe(auctionId);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task PostStripe_UnauthorizedUser_ReturnsForbid()
        {
            var auctionId = Guid.NewGuid();
            var buyerId = Guid.NewGuid().ToString();

            var auction = new Auction { Id = auctionId };
            var purchase = new Purchase { BuyerId = Guid.NewGuid().ToString() };

            _auctionsRepositoryMock.Setup(repo => repo.GetAsync(auctionId)).ReturnsAsync(auction);
            _purchaseRepositoryMock.Setup(repo => repo.GetLastAsync(auctionId)).ReturnsAsync(purchase);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, buyerId)
            };
            var identity = new ClaimsIdentity(claims);
            var userPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userPrincipal }
            };

            var result = await _controller.PostStripe(auctionId);

            Assert.IsType<ForbidResult>(result.Result);
        }
    }
}
