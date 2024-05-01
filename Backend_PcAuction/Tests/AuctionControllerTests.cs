using Backend_PcAuction.Controllers;
using Backend_PcAuction.Data.Dtos;
using Backend_PcAuction.Data.Entities;
using Backend_PcAuction.Data.Repositories;
using Backend_PcAuction.Services;
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
    public class AuctionControllerTests
    {
        private readonly Mock<IAuctionsRepository> _auctionsRepositoryMock;
        private readonly Mock<IPartsRepository> _partsRepositoryMock;
        private readonly Mock<IPartCategoriesRepository> _partCategoriesRepositoryMock;
        private readonly Mock<IAuthorizationService> _authorizationServiceMock;
        private readonly Mock<IAzureBlobStorageService> _azureBlobStorageServiceMock;
        private readonly Mock<IAuctionService> _auctionServiceMock;
        private readonly Mock<IBidsRepository> _bidsRepositoryMock;
        private readonly Mock<HttpContext> _httpContextMock;

        private readonly AuctionsController _controller;

        public AuctionControllerTests()
        {
            _auctionsRepositoryMock = new Mock<IAuctionsRepository>();
            _partsRepositoryMock = new Mock<IPartsRepository>();
            _partCategoriesRepositoryMock = new Mock<IPartCategoriesRepository>();
            _authorizationServiceMock = new Mock<IAuthorizationService>();
            _azureBlobStorageServiceMock = new Mock<IAzureBlobStorageService>();
            _auctionServiceMock = new Mock<IAuctionService>();
            _bidsRepositoryMock = new Mock<IBidsRepository>();

            _httpContextMock = new Mock<HttpContext>();


            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, "userId")
            };

            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);
            _httpContextMock.SetupGet(c => c.User).Returns(principal);

            _controller = new AuctionsController(_auctionsRepositoryMock.Object, _partsRepositoryMock.Object, _partCategoriesRepositoryMock.Object, _authorizationServiceMock.Object,
                _azureBlobStorageServiceMock.Object, _auctionServiceMock.Object, _bidsRepositoryMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = _httpContextMock.Object
                }
            };
        }

        [Fact]
        public async Task GetRecommendations_ReturnsOk_WithRecommendations()
        {
            var auctionId = Guid.NewGuid();
            var expectedPartCategory = new PartCategory { Id = "CPU" };
            var expectedAuction = new Auction
            {
                Part = new Part { Category = expectedPartCategory }, 
            };
            var expectedRecommendations = new List<Auction>();
            var expectedResultAuctions = expectedRecommendations.Select(auction =>
                new AuctionWithPartNameDto(Guid.NewGuid(), "", "", DateTime.Now, DateTime.Now, DateTime.Now, 0, "", "", "", "", "", "", ""));

            _auctionsRepositoryMock.Setup(repo => repo.GetAsync(auctionId)).ReturnsAsync(expectedAuction);
            _auctionServiceMock.Setup(service => service.GenerateAuctionRecommendations(expectedAuction))
                               .ReturnsAsync(expectedRecommendations);

            var result = await _controller.GetRecommendations(auctionId);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var resultAuctions = Assert.IsAssignableFrom<IEnumerable<AuctionWithPartNameDto>>(okResult.Value);
            Assert.Equal(expectedResultAuctions.Count(), resultAuctions.Count());
        }

        [Fact]
        public async Task GetRecommendations_ReturnsNotFound_WhenAuctionNotFound()
        {
            var auctionId = Guid.NewGuid();
            _auctionsRepositoryMock.Setup(repo => repo.GetAsync(auctionId)).ReturnsAsync((Auction)null);

            var result = await _controller.GetRecommendations(auctionId);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CancelAuction_ReturnsNotFound_WhenAuctionNotFound()
        {
            var auctionId = Guid.NewGuid();
            _auctionsRepositoryMock.Setup(repo => repo.GetAsync(auctionId)).ReturnsAsync((Auction)null);

            var result = await _controller.CancelAuction(auctionId);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CancelAuction_ReturnsUnprocessableEntity_WhenAuctionHasBids()
        {
            var auctionId = Guid.NewGuid();
            var auction = new Auction { Id = auctionId, Status = "Active" };
            _auctionsRepositoryMock.Setup(repo => repo.GetAsync(auctionId)).ReturnsAsync(auction);
            _bidsRepositoryMock.Setup(repo => repo.GetLastAsync(auction.Id)).ReturnsAsync(new Bid());

            var result = await _controller.CancelAuction(auctionId);

            var unprocessableEntityResult = Assert.IsType<UnprocessableEntityObjectResult>(result.Result);
            Assert.Equal("Can not cancel auctions with bids", unprocessableEntityResult.Value);
        }

        [Fact]
        public async Task CancelAuction_ReturnsOk_WhenAuctionIsSuccessfullyCanceled()
        {
            var auctionId = Guid.NewGuid();
            var partId = Guid.NewGuid();
            var categoryId = "CPU";
            var auction = new Auction { Id = auctionId, Status = "Active", Part = new Part { Id = partId, Category = new PartCategory { Id = categoryId } } };
            _auctionsRepositoryMock.Setup(repo => repo.GetAsync(auctionId)).ReturnsAsync(auction);
            _bidsRepositoryMock.Setup(repo => repo.GetLastAsync(auction.Id)).ReturnsAsync((Bid)null);

            var result = await _controller.CancelAuction(auctionId);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var auctionDto = Assert.IsType<AuctionDto>(okResult.Value);
            Assert.Equal(auctionId, auctionDto.Id);
            Assert.Equal(AuctionStatuses.Cancelled, auctionDto.Status);
            Assert.Equal(partId, auctionDto.PartId);
            Assert.Equal(categoryId, auctionDto.CategoryId);
        }

        [Fact]
        public async Task UpdateAuctionsPart_ReturnsOk_WhenAuctionPartIsSuccessfullyUpdated()
        {
            var auctionId = Guid.NewGuid();
            var partId = Guid.NewGuid();
            var categoryId = "GPU";
            var updateAuctionPartDto = new UpdateAuctionPartDto(categoryId, partId);
            var auction = new Auction { Id = auctionId, Status = AuctionStatuses.NewNA };
            _auctionsRepositoryMock.Setup(repo => repo.GetAsync(auctionId)).ReturnsAsync(auction);
            _authorizationServiceMock.Setup(service => service.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), auction, PolicyNames.ResourceOwner)).ReturnsAsync(AuthorizationResult.Success);
            _partsRepositoryMock.Setup(repo => repo.GetAsync(updateAuctionPartDto.CategoryId, updateAuctionPartDto.PartId)).ReturnsAsync(new Part { Id = partId, Category = new PartCategory { Id = categoryId } });

            var result = await _controller.UpdateAuctionsPart(auctionId, updateAuctionPartDto);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var auctionDto = Assert.IsType<AuctionDto>(okResult.Value);
            Assert.Equal(auctionId, auctionDto.Id);
            Assert.Equal(partId, auctionDto.PartId);
            Assert.Equal(categoryId, auctionDto.CategoryId);
            Assert.Equal(AuctionStatuses.New, auctionDto.Status);
        }

        [Fact]
        public async Task UpdateAuctionsPart_ReturnsNotFound_WhenAuctionNotFound()
        {
            var nonExistentAuctionId = Guid.NewGuid();
            var updateAuctionPartDto = new UpdateAuctionPartDto("Cat1", Guid.NewGuid());
            _auctionsRepositoryMock.Setup(repo => repo.GetAsync(nonExistentAuctionId)).ReturnsAsync((Auction)null);

            var result = await _controller.UpdateAuctionsPart(nonExistentAuctionId, updateAuctionPartDto);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task UpdateAuctionsPart_ReturnsForbid_WhenAuthorizationFails()
        {
            var auctionId = Guid.NewGuid();
            var updateAuctionPartDto = new UpdateAuctionPartDto("Cat1", Guid.NewGuid());
            var auction = new Auction { Id = auctionId };
            _auctionsRepositoryMock.Setup(repo => repo.GetAsync(auctionId)).ReturnsAsync(auction);
            _authorizationServiceMock.Setup(service => service.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), auction, PolicyNames.ResourceOwner)).ReturnsAsync(AuthorizationResult.Failed());

            var result = await _controller.UpdateAuctionsPart(auctionId, updateAuctionPartDto);

            Assert.IsType<ForbidResult>(result.Result);
        }

        [Fact]
        public async Task UpdateAuctionsPart_ReturnsNotFound_WhenPartNotFound()
        {
            var auctionId = Guid.NewGuid();
            var updateAuctionPartDto = new UpdateAuctionPartDto("Cat1", Guid.NewGuid());
            var auction = new Auction { Id = auctionId };
            _auctionsRepositoryMock.Setup(repo => repo.GetAsync(auctionId)).ReturnsAsync(auction);
            _authorizationServiceMock.Setup(service => service.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), auction, PolicyNames.ResourceOwner)).ReturnsAsync(AuthorizationResult.Success);
            _partsRepositoryMock.Setup(repo => repo.GetAsync(updateAuctionPartDto.CategoryId, updateAuctionPartDto.PartId)).ReturnsAsync((Part)null);

            var result = await _controller.UpdateAuctionsPart(auctionId, updateAuctionPartDto);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Update_ReturnsOk_WhenAuctionCanBeEdited()
        {
            var auctionId = Guid.NewGuid();
            var updateAuctionDto = new UpdateAuctionDto("Updated Name", "Updated Description", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), 10, "Updated Condition", "Updated Manufacturer", null);

            var partId = Guid.NewGuid();
            var categoryId = "Cat1";
            var auction = new Auction
            {
                Id = auctionId,
                Status = "Active",
                Part = new Part { Id = partId, Category = new PartCategory { Id = categoryId } }
            };
            _auctionsRepositoryMock.Setup(repo => repo.GetAsync(auctionId)).ReturnsAsync(auction);
            _authorizationServiceMock.Setup(service => service.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), auction, PolicyNames.ResourceOwner)).ReturnsAsync(AuthorizationResult.Success());

            var result = await _controller.Update(auctionId, updateAuctionDto);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var auctionDto = Assert.IsType<AuctionDto>(okResult.Value);
            Assert.Equal(updateAuctionDto.Name, auctionDto.Name);
            Assert.Equal(updateAuctionDto.Description, auctionDto.Description);
            Assert.Equal(updateAuctionDto.StartDate, auctionDto.StartDate);
            Assert.Equal(updateAuctionDto.EndDate, auctionDto.EndDate);
            Assert.Equal(updateAuctionDto.MinIncrement, auctionDto.MinIncrement);
            Assert.Equal(updateAuctionDto.Condition, auctionDto.Condition);
            Assert.Equal(updateAuctionDto.Manufacturer, auctionDto.Manufacturer);
            Assert.Equal(partId, auctionDto.PartId);
            Assert.Equal(categoryId, auctionDto.CategoryId);
        }







    }
}
