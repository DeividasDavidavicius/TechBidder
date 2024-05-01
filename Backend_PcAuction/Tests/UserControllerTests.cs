using Backend_PcAuction.Controllers;
using Backend_PcAuction.Data.Dtos;
using Backend_PcAuction.Data.Entities;
using Backend_PcAuction.Data.Repositories;
using Backend_PcAuction.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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
    public class UserControllerTests
    {
        private readonly Mock<IAuctionsRepository> _auctionsRepositoryMock;
        private readonly Mock<IBidsRepository> _bidsRepositoryMock;
        private readonly Mock<IPurchaseRepository> _purchaseRepositoryMock;
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<HttpContext> _httpContextMock;

        private readonly UserController _controller;


        public UserControllerTests()
        {
            _auctionsRepositoryMock = new Mock<IAuctionsRepository>();
            _bidsRepositoryMock = new Mock<IBidsRepository>();
            _purchaseRepositoryMock = new Mock<IPurchaseRepository>();

            var mockUserStore = new Mock<IUserStore<User>>();
            var mockPasswordHasher = new Mock<IPasswordHasher<User>>();

            _userManagerMock = new Mock<UserManager<User>>(
                mockUserStore.Object, null, mockPasswordHasher.Object, null, null, null, null, null, null);

            _httpContextMock = new Mock<HttpContext>();
            _httpContextMock.SetupGet(c => c.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, "userId")
            }, "mock")));

            _controller = new UserController(_auctionsRepositoryMock.Object, _bidsRepositoryMock.Object, _purchaseRepositoryMock.Object, _userManagerMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = _httpContextMock.Object
                }
            };
        }

        [Fact]
        public async Task Get_ExistingUser_ReturnsUserDto()
        {
            var userId = "userId";
            var userName = "userName";
            var userAddress = "userAddress";
            var userPhoneNumber = "userPhoneNumber";
            var userBankDetails = "userBankDetails";

            var user = new User
            {
                Id = userId,
                UserName = userName,
                Address = userAddress,
                PhoneNumber = userPhoneNumber,
                BankDetails = userBankDetails
            };

            _userManagerMock.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(user);

            var result = await _controller.Get(userId);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var userDto = Assert.IsType<UserDto>(okResult.Value);
            Assert.Equal(userId, userDto.Id);
            Assert.Equal(userName, userDto.Username);
            Assert.Equal(userAddress, userDto.Address);
            Assert.Equal(userPhoneNumber, userDto.PhoneNumber);
            Assert.Equal(userBankDetails, userDto.BankDetails);
        }

        [Fact]
        public async Task Get_NonExistingUser_ReturnsNotFound()
        {
            var userId = "nonExistingUserId";

            _userManagerMock.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync((User)null);

            var result = await _controller.Get(userId);

            var notFoundResult = Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Update_ValidUser_ReturnsOkWithUpdatedUserDto()
        {
            var updateUserDto = new UpdateUserDto("newAddress", "newPhoneNumber", "newBankDetails");

            var existingUser = new User
            {
                Id = "userId",
                UserName = "username",
                Address = "oldAddress",
                PhoneNumber = "oldPhoneNumber",
                BankDetails = "oldBankDetails"
            };

            _userManagerMock.Setup(m => m.FindByIdAsync("userId")).ReturnsAsync(existingUser);

            var result = await _controller.Update(updateUserDto);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var userDto = Assert.IsType<UserDto>(okResult.Value);
            Assert.Equal("userId", userDto.Id);
            Assert.Equal("newAddress", userDto.Address);
            Assert.Equal("newPhoneNumber", userDto.PhoneNumber);
            Assert.Equal("newBankDetails", userDto.BankDetails);

            Assert.Equal("newAddress", existingUser.Address);
            Assert.Equal("newPhoneNumber", existingUser.PhoneNumber);
            Assert.Equal("newBankDetails", existingUser.BankDetails);
            _userManagerMock.Verify(u => u.UpdateAsync(existingUser), Times.Once);


        }

        [Fact]
        public async Task Update_InvalidUser_ReturnsUnprocessableEntity()
        {
            var updateUserDto = new UpdateUserDto("newAddress", "newPhoneNumber", "newBankDetails");

            _userManagerMock.Setup(m => m.FindByIdAsync("userId")).ReturnsAsync((User)null);

            var result = await _controller.Update(updateUserDto);

            var unprocessableEntityResult = Assert.IsType<UnprocessableEntityObjectResult>(result.Result);
            Assert.Equal("Invalid token", unprocessableEntityResult.Value);
        }

        [Fact]
        public async Task GetAllBids_ReturnsOkWithBidWithAuctionIdDtoList()
        {
            var userId = "userId";
            var bids = new List<Bid>
            {
                new Bid { Id = Guid.NewGuid(), Amount = 100, CreationDate = DateTime.Now, Auction = new Auction { Id = Guid.NewGuid(), Name = "Auction1" } },
                new Bid { Id = Guid.NewGuid(), Amount = 150, CreationDate = DateTime.Now, Auction = new Auction { Id = Guid.NewGuid(), Name = "Auction2" } }
            };

            _bidsRepositoryMock.Setup(repo => repo.GetAllByUserAsync(userId)).ReturnsAsync(bids);

            var result = await _controller.GetAllBids();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var bidWithAuctionIdDtoList = Assert.IsAssignableFrom<IEnumerable<BidWithAuctionIdDto>>(okResult.Value);
            Assert.Equal(bids.Count, bidWithAuctionIdDtoList.Count());
            foreach (var bidWithAuctionIdDto in bidWithAuctionIdDtoList)
            {
                var bid = bids.FirstOrDefault(b => b.Id == bidWithAuctionIdDto.Id);
                Assert.NotNull(bid);
                Assert.Equal(bid.Amount, bidWithAuctionIdDto.Amount);
                Assert.Equal(bid.CreationDate, bidWithAuctionIdDto.CreationDate);
                Assert.Equal(bid.Auction.Id, bidWithAuctionIdDto.AuctionId);
                Assert.Equal(bid.Auction.Name, bidWithAuctionIdDto.AuctionName);
            }
        }

        [Fact]
        public async Task GetAllWinningBids_ReturnsOkWithWinningBidWithAuctionIdDtoList()
        {
            var userId = "userId";
            var bids = new List<Bid>
            {
                new Bid { Id = Guid.NewGuid(), Amount = 100, CreationDate = DateTime.Now, Auction = new Auction { Id = Guid.NewGuid(), Name = "Auction1", Status = AuctionStatuses.Active } },
                new Bid { Id = Guid.NewGuid(), Amount = 120, CreationDate = DateTime.Now, Auction = new Auction { Id = Guid.NewGuid(), Name = "Auction2", Status = AuctionStatuses.ActiveNA } },
            };

            _bidsRepositoryMock.Setup(repo => repo.GetAllByUserAsync(userId)).ReturnsAsync(bids);
            _bidsRepositoryMock.Setup(repo => repo.GetLastAsync(It.IsAny<Guid>())).ReturnsAsync((Guid auctionId) =>
            {
                return bids.OrderByDescending(b => b.Amount).FirstOrDefault(b => b.Auction.Id == auctionId);
            });

            var result = await _controller.GetAllWinningBids();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var winningBidWithAuctionIdDtoList = Assert.IsAssignableFrom<IEnumerable<BidWithAuctionIdDto>>(okResult.Value);
            Assert.Equal(2, winningBidWithAuctionIdDtoList.Count());
        }

        [Fact]
        public async Task GetAllNewAuctions_ReturnsOkWithAuctionWithPartNameDtoList()
        {
            var userId = "userId";
            var auctions = new List<Auction>
            {
                new Auction { Id = Guid.NewGuid(), Name = "Auction1", Part = new Part { Id = Guid.NewGuid(), Name = "Part1", Category = new PartCategory { Id = "1" } } },
                new Auction { Id = Guid.NewGuid(), Name = "Auction2", Part = new Part { Id = Guid.NewGuid(), Name = "Part2", Category = new PartCategory { Id = "2" } } }
            };

            _auctionsRepositoryMock.Setup(repo => repo.GetAllNewByUserAsync(userId)).ReturnsAsync(auctions);

            var result = await _controller.GetAllNewAuctions();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var auctionWithPartNameDtoList = Assert.IsAssignableFrom<IEnumerable<AuctionWithPartNameDto>>(okResult.Value);
            Assert.Equal(2, auctionWithPartNameDtoList.Count());
        }

        [Fact]
        public async Task GetAllActiveAuctions_ReturnsOkWithAuctionWithPartNameDtoList()
        {
            var userId = "userId";
            var auctions = new List<Auction>
            {
                new Auction { Id = Guid.NewGuid(), Name = "Auction1", Part = new Part { Id = Guid.NewGuid(), Name = "Part1", Category = new PartCategory { Id = "1" } } },
                new Auction { Id = Guid.NewGuid(), Name = "Auction2", Part = new Part { Id = Guid.NewGuid(), Name = "Part2", Category = new PartCategory { Id = "2" } } }
            };

            _auctionsRepositoryMock.Setup(repo => repo.GetAllActiveByUserAsync(userId)).ReturnsAsync(auctions);

            var result = await _controller.GetAllActiveAuctions();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var auctionWithPartNameDtoList = Assert.IsAssignableFrom<IEnumerable<AuctionWithPartNameDto>>(okResult.Value);
            Assert.Equal(2, auctionWithPartNameDtoList.Count());
        }

        [Fact]
        public async Task GetAllEndedAuctions_ReturnsOkWithAuctionWithPartNameDtoList()
        {
            var userId = "userId";
            var auctions = new List<Auction>
            {
                new Auction { Id = Guid.NewGuid(), Name = "Auction1", Part = new Part { Id = Guid.NewGuid(), Name = "Part1", Category = new PartCategory { Id = "1" } } },
                new Auction { Id = Guid.NewGuid(), Name = "Auction2", Part = new Part { Id = Guid.NewGuid(), Name = "Part2", Category = new PartCategory { Id = "2" } } }
            };

            _auctionsRepositoryMock.Setup(repo => repo.GetAllEndedByUserAsync(userId)).ReturnsAsync(auctions);

            var result = await _controller.GetAllEndedAuctions();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var auctionWithPartNameDtoList = Assert.IsAssignableFrom<IEnumerable<AuctionWithPartNameDto>>(okResult.Value);
            Assert.Equal(2, auctionWithPartNameDtoList.Count());
        }

        [Fact]
        public async Task GetAllWonAuctions_ReturnsOkWithAuctionWithPartNameDtoList()
        {
            var userId = "userId";
            var purchases = new List<Purchase>
            {
                new Purchase { Id = Guid.NewGuid(), Auction = new Auction { Id = Guid.NewGuid(), Name = "Auction1", Part = new Part { Id = Guid.NewGuid(), Name = "Part1", Category = new PartCategory { Id = "1" } } } },
                new Purchase { Id = Guid.NewGuid(), Auction = new Auction { Id = Guid.NewGuid(), Name = "Auction2", Part = new Part { Id = Guid.NewGuid(), Name = "Part2", Category = new PartCategory { Id = "2" } } } }
            };

            _purchaseRepositoryMock.Setup(repo => repo.GetAllUserPurchasesAsync(userId)).ReturnsAsync(purchases);

            var result = await _controller.GetAllWonAuctions();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var auctionWithPartNameDtoList = Assert.IsAssignableFrom<IEnumerable<AuctionWithPartNameDto>>(okResult.Value);
            Assert.Equal(2, auctionWithPartNameDtoList.Count());
        }
    }
}
