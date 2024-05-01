using Backend_PcAuction.Controllers;
using Backend_PcAuction.Data.Dtos;
using Backend_PcAuction.Data.Entities;
using Backend_PcAuction.Data.Repositories;
using Backend_PcAuction.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
    public class BidsControllerTests
    {
        private readonly Mock<IAuctionsRepository> _auctionsRepositoryMock;
        private readonly Mock<IBidsRepository> _bidsRepositoryMock;
        private readonly Mock<IAuthorizationService> _authorizationServiceMock;
        private readonly Mock<HttpContext> _httpContextMock;

        private readonly BidsController _controller;

        public BidsControllerTests()
        {
            _auctionsRepositoryMock = new Mock<IAuctionsRepository>();
            _bidsRepositoryMock = new Mock<IBidsRepository>();
            _authorizationServiceMock = new Mock<IAuthorizationService>();
            _httpContextMock = new Mock<HttpContext>();

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, "userId")
            };

            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            _httpContextMock.SetupGet(c => c.User).Returns(principal);

            _controller = new BidsController(_auctionsRepositoryMock.Object, _bidsRepositoryMock.Object, _authorizationServiceMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = _httpContextMock.Object
                }
            };
        }

        [Fact]
        public async Task Create_ReturnsCreated_WhenValidInputAndActiveAuction()
        {
            var auctionId = Guid.NewGuid();
            var createBidDto = new CreateBidDto(100);
            var userId = Guid.NewGuid().ToString();
            var auction = new Auction { Id = auctionId, Status = AuctionStatuses.Active, EndDate = DateTime.UtcNow.AddDays(1), UserId = userId };
            _auctionsRepositoryMock.Setup(repo => repo.GetAsync(auctionId)).ReturnsAsync(auction);

            var result = await _controller.Create(auctionId, createBidDto);

            var createdResult = Assert.IsType<CreatedResult>(result.Result);
            var bidDto = Assert.IsType<BidDto>(createdResult.Value);
            Assert.Equal(createBidDto.Amount, bidDto.Amount);
        }

        [Fact]
        public async Task Create_ReturnsUnprocessableEntity_WhenAuctionNotActive()
        {
            var auctionId = Guid.NewGuid();
            var createBidDto = new CreateBidDto(100);
            var auction = new Auction { Status = AuctionStatuses.New };
            _auctionsRepositoryMock.Setup(repo => repo.GetAsync(auctionId)).ReturnsAsync(auction);

            var result = await _controller.Create(auctionId, createBidDto);

            Assert.IsType<UnprocessableEntityObjectResult>(result.Result);
        }

        [Fact]
        public async Task Create_ReturnsNotFound_WhenAuctionNotFound()
        {
            var auctionId = Guid.NewGuid();
            var createBidDto = new CreateBidDto(100);
            _auctionsRepositoryMock.Setup(repo => repo.GetAsync(auctionId)).ReturnsAsync((Auction)null);

            var result = await _controller.Create(auctionId, createBidDto);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Create_ReturnsUnprocessableEntity_WhenMinIncrementNotMet()
        {
            var auctionId = Guid.NewGuid();
            var createBidDto = new CreateBidDto(50);
            var auction = new Auction { Status = AuctionStatuses.Active, MinIncrement = 10 };
            _auctionsRepositoryMock.Setup(repo => repo.GetAsync(auctionId)).ReturnsAsync(auction);
            _bidsRepositoryMock.Setup(repo => repo.GetLastAsync(auctionId)).ReturnsAsync(new Bid { Amount = 40 });

            var result = await _controller.Create(auctionId, createBidDto);

            Assert.IsType<UnprocessableEntityObjectResult>(result.Result);
        }

        [Fact]
        public async Task Create_ReturnsUnprocessableEntity_WhenAmountIsZero()
        {
            var auctionId = Guid.NewGuid();
            var createBidDto = new CreateBidDto(0);
            var auction = new Auction { Status = AuctionStatuses.Active, MinIncrement = 10 };
            _auctionsRepositoryMock.Setup(repo => repo.GetAsync(auctionId)).ReturnsAsync(auction);

            var result = await _controller.Create(auctionId, createBidDto);

            Assert.IsType<UnprocessableEntityObjectResult>(result.Result);
        }

        [Fact]
        public async Task Create_ReturnsUnprocessableEntity_WhenAmountExceedsLimit()
        {
            var auctionId = Guid.NewGuid();
            var createBidDto = new CreateBidDto(60000);
            var auction = new Auction { Status = AuctionStatuses.Active, MinIncrement = 10 };
            _auctionsRepositoryMock.Setup(repo => repo.GetAsync(auctionId)).ReturnsAsync(auction);

            var result = await _controller.Create(auctionId, createBidDto);

            Assert.IsType<UnprocessableEntityObjectResult>(result.Result);
        }

        [Fact]
        public async Task Create_ReturnsUnprocessableEntity_WhenBidNotHigherByMinIncrementThanPreviousBid()
        {
            var auctionId = Guid.NewGuid();
            var createBidDto = new CreateBidDto(55);
            var auction = new Auction { Status = AuctionStatuses.Active, MinIncrement = 10 };
            var lastBid = new Bid { Amount = 50 };
            _auctionsRepositoryMock.Setup(repo => repo.GetAsync(auctionId)).ReturnsAsync(auction);
            _bidsRepositoryMock.Setup(repo => repo.GetLastAsync(auctionId)).ReturnsAsync(lastBid);

            var result = await _controller.Create(auctionId, createBidDto);

            Assert.IsType<UnprocessableEntityObjectResult>(result.Result);
        }


        [Fact]
        public async Task Create_ReturnsUnprocessableEntity_WhenBidNotHigherThanPreviousBid()
        {
            var auctionId = Guid.NewGuid();
            var createBidDto = new CreateBidDto(50);
            var auction = new Auction { Status = AuctionStatuses.Active, MinIncrement = 0 };
            var lastBid = new Bid { Amount = 50 };
            _auctionsRepositoryMock.Setup(repo => repo.GetAsync(auctionId)).ReturnsAsync(auction);
            _bidsRepositoryMock.Setup(repo => repo.GetLastAsync(auctionId)).ReturnsAsync(lastBid);

            var result = await _controller.Create(auctionId, createBidDto);

            Assert.IsType<UnprocessableEntityObjectResult>(result.Result);
        }

        [Fact]
        public async Task Create_ReturnsUnprocessableEntity_WhenStartingBidNotPositiveNumber()
        {
            var auctionId = Guid.NewGuid();
            var createBidDto = new CreateBidDto(0); // Zero bid amount
            var auction = new Auction { Status = AuctionStatuses.Active, MinIncrement = 0 };
            _auctionsRepositoryMock.Setup(repo => repo.GetAsync(auctionId)).ReturnsAsync(auction);

            var result = await _controller.Create(auctionId, createBidDto);

            Assert.IsType<UnprocessableEntityObjectResult>(result.Result);
        }

        [Fact]
        public async Task Create_ReturnsUnprocessableEntity_WhenUserBidsOnOwnAuction()
        {
            var auctionId = Guid.NewGuid();
            var createBidDto = new CreateBidDto(100);
            var userId = Guid.NewGuid().ToString();
            var auction = new Auction { Id = auctionId, Status = AuctionStatuses.Active, EndDate = DateTime.UtcNow.AddDays(1), UserId = "userId" };
            _auctionsRepositoryMock.Setup(repo => repo.GetAsync(auctionId)).ReturnsAsync(auction);

            var result = await _controller.Create(auctionId, createBidDto);

            Assert.IsType<UnprocessableEntityObjectResult>(result.Result);
        }

        [Fact]
        public async Task Get_ReturnsOk_WhenAuctionAndBidExist()
        {
            var auctionId = Guid.NewGuid();
            var bidId = Guid.NewGuid();
            var auction = new Auction { Id = auctionId };
            var bid = new Bid { Id = bidId };
            _auctionsRepositoryMock.Setup(repo => repo.GetAsync(auctionId)).ReturnsAsync(auction);
            _bidsRepositoryMock.Setup(repo => repo.GetAsync(auctionId, bidId)).ReturnsAsync(bid);

            var result = await _controller.Get(auctionId, bidId);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var bidDto = Assert.IsType<BidDto>(okResult.Value);
            Assert.Equal(bid.Id, bidDto.Id);
            Assert.Equal(bid.Amount, bidDto.Amount);
            Assert.Equal(bid.CreationDate, bidDto.CreationDate);
        }

        [Fact]
        public async Task Get_ReturnsNotFound_WhenAuctionExistsButBidDoesNotExist()
        {
            var auctionId = Guid.NewGuid();
            var bidId = Guid.NewGuid();
            var auction = new Auction { Id = auctionId };
            _auctionsRepositoryMock.Setup(repo => repo.GetAsync(auctionId)).ReturnsAsync(auction);
            _bidsRepositoryMock.Setup(repo => repo.GetAsync(auctionId, bidId)).ReturnsAsync((Bid)null);

            var result = await _controller.Get(auctionId, bidId);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Get_ReturnsNotFound_WhenAuctionDoesNotExist()
        {
            var auctionId = Guid.NewGuid();
            var bidId = Guid.NewGuid();
            _auctionsRepositoryMock.Setup(repo => repo.GetAsync(auctionId)).ReturnsAsync((Auction)null);

            var result = await _controller.Get(auctionId, bidId);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetMany_ReturnsOk_WhenAuctionExists()
        {
            var auctionId = Guid.NewGuid();
            var auction = new Auction { Id = auctionId };
            var bids = new List<Bid>
            {
                new Bid { Id = Guid.NewGuid(), Amount = 100, CreationDate = DateTime.UtcNow, UserId = "user1" },
                new Bid { Id = Guid.NewGuid(), Amount = 150, CreationDate = DateTime.UtcNow, UserId = "user2" }
            };

            foreach (var bid in bids)
            {
                bid.User = new User { UserName = "Username" };
            }

            _auctionsRepositoryMock.Setup(repo => repo.GetAsync(auctionId)).ReturnsAsync(auction);
            _bidsRepositoryMock.Setup(repo => repo.GetManyAsync(auctionId)).ReturnsAsync(bids);

            var result = await _controller.GetMany(auctionId);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var bidDtos = Assert.IsAssignableFrom<IEnumerable<BidWithUsernameDto>>(okResult.Value);
            Assert.Equal(bids.Count, bidDtos.Count());
        }


        [Fact]
        public async Task GetMany_ReturnsNotFound_WhenAuctionDoesNotExist()
        {
            var auctionId = Guid.NewGuid();
            _auctionsRepositoryMock.Setup(repo => repo.GetAsync(auctionId)).ReturnsAsync((Auction)null);

            var result = await _controller.GetMany(auctionId);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Delete_ReturnsNoContent_WhenBidDeletedSuccessfully()
        {
            var auctionId = Guid.NewGuid();
            var bidId = Guid.NewGuid();
            var userId = "user123";
            var auction = new Auction { Id = auctionId, EndDate = DateTime.UtcNow.AddDays(1) };
            var bid = new Bid { Id = bidId, UserId = userId };

            _auctionsRepositoryMock.Setup(repo => repo.GetAsync(auctionId)).ReturnsAsync(auction);
            _bidsRepositoryMock.Setup(repo => repo.GetAsync(auctionId, bidId)).ReturnsAsync(bid);
            _authorizationServiceMock.Setup(service => service.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), bid, PolicyNames.ResourceOwner))
                .ReturnsAsync(AuthorizationResult.Success());

            var result = await _controller.Delete(auctionId, bidId);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenAuctionNotFound()
        {
            var auctionId = Guid.NewGuid();
            var bidId = Guid.NewGuid();
            _auctionsRepositoryMock.Setup(repo => repo.GetAsync(auctionId)).ReturnsAsync((Auction)null);

            var result = await _controller.Delete(auctionId, bidId);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenBidNotFound()
        {
            var auctionId = Guid.NewGuid();
            var bidId = Guid.NewGuid();
            var auction = new Auction { Id = auctionId };
            _auctionsRepositoryMock.Setup(repo => repo.GetAsync(auctionId)).ReturnsAsync(auction);
            _bidsRepositoryMock.Setup(repo => repo.GetAsync(auctionId, bidId)).ReturnsAsync((Bid)null);

            var result = await _controller.Delete(auctionId, bidId);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsForbid_WhenAuthorizationFails()
        {
            var auctionId = Guid.NewGuid();
            var bidId = Guid.NewGuid();
            var userId = "user123";
            var auction = new Auction { Id = auctionId };
            var bid = new Bid { Id = bidId, UserId = userId };

            _auctionsRepositoryMock.Setup(repo => repo.GetAsync(auctionId)).ReturnsAsync(auction);
            _bidsRepositoryMock.Setup(repo => repo.GetAsync(auctionId, bidId)).ReturnsAsync(bid);
            _authorizationServiceMock.Setup(service => service.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), bid, PolicyNames.ResourceOwner))
                .ReturnsAsync(AuthorizationResult.Failed());

            var result = await _controller.Delete(auctionId, bidId);

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsForbid_WhenAuctionEndDateWithinNext60Minutes()
        {
            var auctionId = Guid.NewGuid();
            var bidId = Guid.NewGuid();
            var userId = "user123";
            var auction = new Auction { Id = auctionId, EndDate = DateTime.UtcNow.AddMinutes(30) };
            var bid = new Bid { Id = bidId, UserId = userId };

            _auctionsRepositoryMock.Setup(repo => repo.GetAsync(auctionId)).ReturnsAsync(auction);
            _bidsRepositoryMock.Setup(repo => repo.GetAsync(auctionId, bidId)).ReturnsAsync(bid);
            _authorizationServiceMock.Setup(service => service.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), bid, PolicyNames.ResourceOwner))
                .ReturnsAsync(AuthorizationResult.Success());

            var result = await _controller.Delete(auctionId, bidId);

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task GetHighest_ReturnsHighestBid_WhenBothAuctionAndBidExist()
        {
            var auctionId = Guid.NewGuid();
            var bidId = Guid.NewGuid();
            var amount = 100;
            var creationDate = DateTime.UtcNow;
            var auction = new Auction { Id = auctionId };
            var bid = new Bid { Id = bidId, Amount = amount, CreationDate = creationDate };

            _auctionsRepositoryMock.Setup(repo => repo.GetAsync(auctionId)).ReturnsAsync(auction);
            _bidsRepositoryMock.Setup(repo => repo.GetLastAsync(auctionId)).ReturnsAsync(bid);

            var result = await _controller.GetHighest(auctionId);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var bidDto = Assert.IsType<BidDto>(okResult.Value);
            Assert.Equal(bidId, bidDto.Id);
            Assert.Equal(amount, bidDto.Amount);
            Assert.Equal(creationDate, bidDto.CreationDate);
        }

        [Fact]
        public async Task GetHighest_ReturnsDefaultBid_WhenNoBidExists()
        {
            var auctionId = Guid.NewGuid();
            var auction = new Auction { Id = auctionId };

            _auctionsRepositoryMock.Setup(repo => repo.GetAsync(auctionId)).ReturnsAsync(auction);
            _bidsRepositoryMock.Setup(repo => repo.GetLastAsync(auctionId)).ReturnsAsync((Bid)null);

            var result = await _controller.GetHighest(auctionId);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var bidDto = Assert.IsType<BidDto>(okResult.Value);
            Assert.Equal(Guid.Empty, bidDto.Id);
            Assert.Equal(-1, bidDto.Amount);
            Assert.True(bidDto.CreationDate < DateTime.UtcNow);
        }

        [Fact]
        public async Task GetHighest_ReturnsNotFound_WhenAuctionNotFound()
        {
            var auctionId = Guid.NewGuid();
            _auctionsRepositoryMock.Setup(repo => repo.GetAsync(auctionId)).ReturnsAsync((Auction)null);

            var result = await _controller.GetHighest(auctionId);

            Assert.IsType<NotFoundResult>(result.Result);
        }
    }
}
