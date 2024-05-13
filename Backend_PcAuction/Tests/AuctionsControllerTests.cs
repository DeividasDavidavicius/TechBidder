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
    public class AuctionsControllerTests
    {
        private readonly Mock<IAuctionsRepository> _auctionsRepositoryMock;
        private readonly Mock<IPartsRepository> _partsRepositoryMock;
        private readonly Mock<IPartCategoriesRepository> _partCategoriesRepositoryMock;
        private readonly Mock<IAuthorizationService> _authorizationServiceMock;
        private readonly Mock<IAzureBlobStorageService> _azureBlobStorageServiceMock;
        private readonly Mock<IAuctionsService> _auctionServiceMock;
        private readonly Mock<IBidsRepository> _bidsRepositoryMock;
        private readonly Mock<HttpContext> _httpContextMock;

        private readonly AuctionsController _controller;

        public AuctionsControllerTests()
        {
            _auctionsRepositoryMock = new Mock<IAuctionsRepository>();
            _partsRepositoryMock = new Mock<IPartsRepository>();
            _partCategoriesRepositoryMock = new Mock<IPartCategoriesRepository>();
            _authorizationServiceMock = new Mock<IAuthorizationService>();
            _azureBlobStorageServiceMock = new Mock<IAzureBlobStorageService>();
            _auctionServiceMock = new Mock<IAuctionsService>();
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

        [Fact]
        public async Task Update_ReturnsNotFound_WhenAuctionNotFound()
        {
            var auctionId = Guid.NewGuid();
            var updateAuctionDto = new UpdateAuctionDto("Updated Name", "Updated Description", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), 10, "Updated Condition", "Updated Manufacturer", null);
            _auctionsRepositoryMock.Setup(repo => repo.GetAsync(auctionId)).ReturnsAsync((Auction)null);

            var result = await _controller.Update(auctionId, updateAuctionDto);

            var notFoundResult = Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Update_ReturnsForbid_WhenAuthorizationFails()
        {
            var auctionId = Guid.NewGuid();
            var updateAuctionDto = new UpdateAuctionDto("Updated Name", "Updated Description", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), 10, "Updated Condition", "Updated Manufacturer", null);
            var auction = new Auction { };
            _auctionsRepositoryMock.Setup(repo => repo.GetAsync(auctionId)).ReturnsAsync(auction);
            _authorizationServiceMock.Setup(service => service.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), auction, PolicyNames.ResourceOwner)).ReturnsAsync(AuthorizationResult.Failed());

            var result = await _controller.Update(auctionId, updateAuctionDto);

            var forbidResult = Assert.IsType<ForbidResult>(result.Result);
        }

        [Theory]
        [InlineData("Short")]
        [InlineData("ThisIsAVeryLongNameThatExceedsFortyFiveCharactersLimit")]
        public async Task Update_ReturnsUnprocessableEntity_WhenNameLengthIsInvalid(string invalidName)
        {
            var auctionId = Guid.NewGuid();
            var updateAuctionDto = new UpdateAuctionDto(invalidName, "Updated Description", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), 10, "Updated Condition", "Updated Manufacturer", null);
            var auction = new Auction { };
            _auctionsRepositoryMock.Setup(repo => repo.GetAsync(auctionId)).ReturnsAsync(auction);
            _authorizationServiceMock.Setup(service => service.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), auction, PolicyNames.ResourceOwner)).ReturnsAsync(AuthorizationResult.Success());

            var result = await _controller.Update(auctionId, updateAuctionDto);

            var unprocessableEntityResult = Assert.IsType<UnprocessableEntityObjectResult>(result.Result);
        }

        [Fact]
        public async Task Update_ReturnsUnprocessableEntity_WhenDescriptionLengthIsInvalid()
        {
            var auctionId = Guid.NewGuid();
            var invalidDescription = new string('A', 9);
            var updateAuctionDto = new UpdateAuctionDto("Updated Name", invalidDescription, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), 10, "Updated Condition", "Updated Manufacturer", null);
            var auction = new Auction { };
            _auctionsRepositoryMock.Setup(repo => repo.GetAsync(auctionId)).ReturnsAsync(auction);
            _authorizationServiceMock.Setup(service => service.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), auction, PolicyNames.ResourceOwner)).ReturnsAsync(AuthorizationResult.Success());

            var result = await _controller.Update(auctionId, updateAuctionDto);

            var unprocessableEntityResult = Assert.IsType<UnprocessableEntityObjectResult>(result.Result);
        }

        [Fact]
        public async Task Update_ReturnsUnprocessableEntity_WhenMinIncrementIsNegative()
        {
            var auctionId = Guid.NewGuid();
            var updateAuctionDto = new UpdateAuctionDto("Updated Name", "Updated Description", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), -100, "Updated Condition", "Updated Manufacturer", null);
            var auction = new Auction { };
            _auctionsRepositoryMock.Setup(repo => repo.GetAsync(auctionId)).ReturnsAsync(auction);
            _authorizationServiceMock.Setup(service => service.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), auction, PolicyNames.ResourceOwner)).ReturnsAsync(AuthorizationResult.Success());

            var result = await _controller.Update(auctionId, updateAuctionDto);

            var unprocessableEntityResult = Assert.IsType<UnprocessableEntityObjectResult>(result.Result);
        }

        [Fact]
        public async Task Update_ReturnsUnprocessableEntity_WhenStartDateIsInThePast()
        {
            var auctionId = Guid.NewGuid();
            var pastStartDate = DateTime.UtcNow.AddDays(-1);
            var updateAuctionDto = new UpdateAuctionDto("Updated Name", "Updated Description", pastStartDate, DateTime.UtcNow.AddDays(2), 10, "Updated Condition", "Updated Manufacturer", null);
            var auction = new Auction { Status = AuctionStatuses.New, /* Populate other fields */ };
            _auctionsRepositoryMock.Setup(repo => repo.GetAsync(auctionId)).ReturnsAsync(auction);
            _authorizationServiceMock.Setup(service => service.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), auction, PolicyNames.ResourceOwner)).ReturnsAsync(AuthorizationResult.Success());

            var result = await _controller.Update(auctionId, updateAuctionDto);

            var unprocessableEntityResult = Assert.IsType<UnprocessableEntityObjectResult>(result.Result);
        }

        [Fact]
        public async Task Update_ReturnsUnprocessableEntity_WhenEndDateIsBeforeCurrentTime()
        {
            var auctionId = Guid.NewGuid();
            var pastEndDate = DateTime.UtcNow.AddDays(-1);
            var updateAuctionDto = new UpdateAuctionDto("Updated Name", "Updated Description", DateTime.UtcNow.AddDays(1), pastEndDate, 10, "Updated Condition", "Updated Manufacturer", null);
            var auction = new Auction { };
            _auctionsRepositoryMock.Setup(repo => repo.GetAsync(auctionId)).ReturnsAsync(auction);
            _authorizationServiceMock.Setup(service => service.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), auction, PolicyNames.ResourceOwner)).ReturnsAsync(AuthorizationResult.Success());

            var result = await _controller.Update(auctionId, updateAuctionDto);

            var unprocessableEntityResult = Assert.IsType<UnprocessableEntityObjectResult>(result.Result);
        }

        [Fact]
        public async Task Update_ReturnsUnprocessableEntity_WhenEndDateIsBeforeStartDate()
        {
            var auctionId = Guid.NewGuid();
            var startDate = DateTime.UtcNow.AddDays(2);
            var endDate = DateTime.UtcNow.AddDays(1);
            var updateAuctionDto = new UpdateAuctionDto("Updated Name", "Updated Description", startDate, endDate, 10, "Updated Condition", "Updated Manufacturer", null);
            var auction = new Auction { };
            _auctionsRepositoryMock.Setup(repo => repo.GetAsync(auctionId)).ReturnsAsync(auction);
            _authorizationServiceMock.Setup(service => service.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), auction, PolicyNames.ResourceOwner)).ReturnsAsync(AuthorizationResult.Success());

            var result = await _controller.Update(auctionId, updateAuctionDto);

            var unprocessableEntityResult = Assert.IsType<UnprocessableEntityObjectResult>(result.Result);
        }

        [Fact]
        public async Task Update_WithImage_ReturnsOk()
        {
            var auctionId = Guid.NewGuid();
            var updateAuctionDto = new UpdateAuctionDto("Updated Name", "Updated Description", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), 10, "Updated Condition", "Updated Manufacturer", new Mock<IFormFile>().Object);


            var partId = Guid.NewGuid();
            var categoryId = "Cat1";
            var auction = new Auction 
            { 
                Id = auctionId, 
                Status = AuctionStatuses.New,
                Part = new Part { Id = partId, Category = new PartCategory { Id = categoryId } },
                ImageUri = "uri"
            };

            _auctionsRepositoryMock.Setup(repo => repo.GetAsync(auctionId)).ReturnsAsync(auction);
            _authorizationServiceMock.Setup(service => service.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), auction, PolicyNames.ResourceOwner))
                .ReturnsAsync(AuthorizationResult.Success());
            _azureBlobStorageServiceMock.Setup(service => service.UploadImageAsync(updateAuctionDto.Image)).ReturnsAsync("image-url");

            var result = await _controller.Update(auctionId, updateAuctionDto);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var auctionDto = Assert.IsType<AuctionDto>(okResult.Value);
            Assert.NotNull(auctionDto);
        }

        [Fact]
        public async Task GetManyWithPagination_ReturnsOk_WhenCorrectSortType()
        {
            var expectedPage = 1;
            var expectedCategoryId = "category1";
            var expectedSeriesId = Guid.NewGuid();
            var expectedPartId = Guid.NewGuid();
            var expectedSortType = AuctionSortingTypes.CreationDate;
            var auctions = new List<Auction>
            {
                new Auction { Id = Guid.NewGuid(), Name = "Auction 1",
                    Part = new Part { Id = Guid.NewGuid(), Name = "Part 1", Category = new PartCategory { Id = "cat1" } } },
                new Auction { Id = Guid.NewGuid(), Name = "Auction 2",
                    Part = new Part { Id = Guid.NewGuid(), Name = "Part 2", Category = new PartCategory { Id = "cat2" } } },
            };
            var auctionCount = auctions.Count;
            _auctionsRepositoryMock.Setup(repo => repo.GetManyWithPaginationAsync(expectedPage, expectedCategoryId, expectedSeriesId, expectedPartId, expectedSortType))
                .ReturnsAsync(auctions);
            _auctionsRepositoryMock.Setup(repo => repo.GetCountAsync(expectedCategoryId, expectedSeriesId, expectedPartId))
                .ReturnsAsync(auctionCount);

            var result = await _controller.GetManyWithPagination(expectedPage, expectedCategoryId, expectedSeriesId, expectedPartId, expectedSortType);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<AuctionsWithPaginationDto>(okResult.Value);
            Assert.Equal(auctions.Count, response.Auctions.Count());
            Assert.Equal(auctionCount, response.AuctionCount);
        }

        [Fact]
        public async Task GetManyWithPagination_WithInvalidSortType_ReturnsUnprocessableEntity()
        {
            var invalidSortType = "invalidSortType";

            var result = await _controller.GetManyWithPagination(sortType: invalidSortType);

            Assert.IsType<UnprocessableEntityResult>(result.Result);
        }

        [Fact]
        public async Task GetMany_Returns_Ok_With_AuctionDtos()
        {
            var partCategory = new PartCategory { Id = "CPU" };
            var part = new Part { Id = Guid.NewGuid(), Name = "Part 1", Category = partCategory };
            var auctions = new List<Auction>
            {
                new Auction { Id = Guid.NewGuid(), Name = "Auction 1", Part = part },
                new Auction { Id = Guid.NewGuid(), Name = "Auction 2", Part = part}
            };

            _auctionsRepositoryMock.Setup(repo => repo.GetManyAsync()).ReturnsAsync(auctions);


            var result = await _controller.GetMany();

            var okResult = Assert.IsType<ActionResult<IEnumerable<AuctionDto>>>(result);
            Assert.IsAssignableFrom<OkObjectResult>(okResult.Result);

            var auctionDtos = Assert.IsAssignableFrom<IEnumerable<AuctionDto>>((okResult.Result as OkObjectResult).Value);
            Assert.Equal(auctions.Count, auctionDtos.Count());
        }

        [Fact]
        public async Task Get_ReturnsOkWithAuctionDto_WhenAuctionExists()
        {
            var auctionId = Guid.NewGuid();
            var partCategory = new PartCategory { Id = "CPU" };
            var part = new Part { Id = Guid.NewGuid(), Name = "Part 1", Category = partCategory };
            var auction = new Auction
            {
                Id = auctionId,
                Name = "Auction 1",
                Part = part
            };

            _auctionsRepositoryMock.Setup(repo => repo.GetAsync(auctionId)).ReturnsAsync(auction);

            var result = await _controller.Get(auctionId);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var auctionDto = Assert.IsType<AuctionDto>(okResult.Value);
            Assert.Equal(auction.Id, auctionDto.Id);
        }

        [Fact]
        public async Task Get_ReturnsNotFound_WhenAuctionDoesNotExist()
        {
            var nonExistingAuctionId = Guid.NewGuid();
            _auctionsRepositoryMock.Setup(repo => repo.GetAsync(nonExistingAuctionId)).ReturnsAsync((Auction)null);

            var result = await _controller.Get(nonExistingAuctionId);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Create_ReturnsCreatedWithAuctionDto_WhenAuctionIsCreatedSuccessfully()
        {
            var createAuctionDto = new CreateAuctionDto(
                "Test Auction",
                "Test Description",
                DateTime.UtcNow.AddDays(1),
                DateTime.UtcNow.AddDays(2),
                10,
                "Test Condition",
                "Test Manufacturer",
                null, 
                "CPU",
                Guid.NewGuid(), 
                "Test1",
                "Test2"
            );

            var auctionId = Guid.NewGuid();
            var partCategory = new PartCategory { Id = "CPU" };
            var part = new Part { Id = (Guid)createAuctionDto.PartId, Name = "Part 1", Category = partCategory };
            var auction = new Auction { Id = auctionId };

            _partsRepositoryMock.Setup(repo => repo.GetAsync(createAuctionDto.PartCategory, createAuctionDto.PartId)).ReturnsAsync(part);
            _auctionsRepositoryMock.Setup(repo => repo.CreateAsync(It.IsAny<Auction>()));

            var result = await _controller.Create(createAuctionDto);

            var createdResult = Assert.IsType<CreatedResult>(result.Result);
            var auctionDto = Assert.IsType<AuctionDto>(createdResult.Value);
            Assert.Equal(createAuctionDto.Name, auctionDto.Name);
        }

        [Fact]
        public async Task Create_ReturnsUnprocessableEntity_WhenNameIsTooShort()
        {
            var createAuctionDto = new CreateAuctionDto(
                "A",
                "Test Description",
                DateTime.UtcNow.AddDays(1),
                DateTime.UtcNow.AddDays(2),
                10,
                "Test Condition",
                "Test Manufacturer",
                null,
                "CPU",
                Guid.NewGuid(),
                "Test1",
                "Test2"
            );

            var result = await _controller.Create(createAuctionDto);

            var unprocessableEntityResult = Assert.IsType<UnprocessableEntityObjectResult>(result.Result);
            Assert.Equal("Title must be 5 - 45 characters long", unprocessableEntityResult.Value);
        }

        [Fact]
        public async Task Create_ReturnsUnprocessableEntity_WhenDescriptionIsTooShort()
        {
            var createAuctionDto = new CreateAuctionDto(
                "Test Auction",
                "Short",
                DateTime.UtcNow.AddDays(1),
                DateTime.UtcNow.AddDays(2),
                10,
                "Test Condition",
                "Test Manufacturer",
                null,
                "CPU",
                Guid.NewGuid(),
                "Test1",
                "Test2"
            );

            var result = await _controller.Create(createAuctionDto);

            var unprocessableEntityResult = Assert.IsType<UnprocessableEntityObjectResult>(result.Result);
            Assert.Equal("Description must be at least 10 characters long", unprocessableEntityResult.Value);
        }

        [Fact]
        public async Task Create_ReturnsUnprocessableEntity_WhenMinIncrementIsNegative()
        {
            var createAuctionDto = new CreateAuctionDto(
                "Test Auction",
                "Test Description",
                DateTime.UtcNow.AddDays(1),
                DateTime.UtcNow.AddDays(2),
                -10,
                "Test Condition",
                "Test Manufacturer",
                null,
                "CPU",
                Guid.NewGuid(),
                "Test1",
                "Test2"
            );

            var result = await _controller.Create(createAuctionDto);

            var unprocessableEntityResult = Assert.IsType<UnprocessableEntityObjectResult>(result.Result);
            Assert.Equal("Minimum increment must 0 or a positive number", unprocessableEntityResult.Value);
        }

        [Fact]
        public async Task Create_ReturnsUnprocessableEntity_WhenStartDateIsInThePast()
        {
            var createAuctionDto = new CreateAuctionDto(
                "Test Auction",
                "Test Description",
                DateTime.UtcNow.AddHours(-1),
                DateTime.UtcNow.AddDays(2),
                10,
                "Test Condition",
                "Test Manufacturer",
                null,
                "CPU",
                Guid.NewGuid(),
                "Test1",
                "Test2"
            );

            var result = await _controller.Create(createAuctionDto);

            var unprocessableEntityResult = Assert.IsType<UnprocessableEntityObjectResult>(result.Result);
            Assert.Equal("Start date must be later than current time", unprocessableEntityResult.Value);
        }

        [Fact]
        public async Task Create_ReturnsUnprocessableEntity_WhenEndDateIsInThePast()
        {
            var createAuctionDto = new CreateAuctionDto(
                "Test Auction",
                "Test Description",
                DateTime.UtcNow.AddDays(1),
                DateTime.UtcNow.AddHours(-1),
                10,
                "Test Condition",
                "Test Manufacturer",
                null,
                "CPU",
                Guid.NewGuid(),
                "Test1",
                "Test2"
            );

            var result = await _controller.Create(createAuctionDto);

            var unprocessableEntityResult = Assert.IsType<UnprocessableEntityObjectResult>(result.Result);
            Assert.Equal("End date must be later than current time", unprocessableEntityResult.Value);
        }

        [Fact]
        public async Task Create_Returns_UnprocessableEntity_WhenEndDateLessThanStartDate()
        {
            var createAuctionDto = new CreateAuctionDto(
                "Test Auction",
                "Test Description",
                DateTime.UtcNow.AddDays(2),
                DateTime.UtcNow.AddDays(1),
                10,
                "Test Condition",
                "Test Manufacturer",
                null,
                "CPU",
                Guid.NewGuid(),
                "Test1",
                "Test2"
            );

            var result = await _controller.Create(createAuctionDto);

            Assert.IsType<UnprocessableEntityObjectResult>(result.Result);
        }

        [Fact]
        public async Task Create_ReturnsNotFound_WhenPartIdIsProvidedButPartDoesNotExist()
        {
            var createAuctionDto = new CreateAuctionDto(
                "Test Auction",
                "Test Description",
                DateTime.UtcNow.AddDays(1),
                DateTime.UtcNow.AddDays(2),
                10,
                "Test Condition",
                "Test Manufacturer",
                null,
                "CPU",
                Guid.NewGuid(),
                "Test1",
                "Test2"
            );

            _partsRepositoryMock.Setup(repo => repo.GetAsync(createAuctionDto.PartCategory, createAuctionDto.PartId)).ReturnsAsync((Part)null);

            var result = await _controller.Create(createAuctionDto);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Part not found", notFoundResult.Value);
        }

        [Fact]
        public async Task Create_ReturnsNotFound_WhenPartCategoryDoesNotExist()
        {
            var createAuctionDto = new CreateAuctionDto(
                "Test Auction",
                "Test Description",
                DateTime.UtcNow.AddDays(1),
                DateTime.UtcNow.AddDays(2),
                10,
                "Test Condition",
                "Test Manufacturer",
                null,
                "NonExistingCategory",
                null,
                "Test1",
                "Test2"
            );

            _partCategoriesRepositoryMock.Setup(repo => repo.GetAsync(createAuctionDto.PartCategoryName)).ReturnsAsync((PartCategory)null);

            var result = await _controller.Create(createAuctionDto);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Create_ReturnsCreatedWithAuctionDto_WhenDataIsCorrect()
        {
            var createAuctionDto = new CreateAuctionDto(
                "Test Auction",
                "Test Description",
                DateTime.UtcNow.AddDays(1),
                DateTime.UtcNow.AddDays(2),
                10,
                "Test Condition",
                "Test Manufacturer",
                null,
                "ExistingCategory",
                null,
                "Test1",
                "Test2"
            );

            var category = new PartCategory { Id = "ExistingCategory" };
            _partCategoriesRepositoryMock.Setup(repo => repo.GetAsync(createAuctionDto.PartCategoryName)).ReturnsAsync(category);

            var result = await _controller.Create(createAuctionDto);

            var createdResult = Assert.IsType<CreatedResult>(result.Result);
            var auctionDto = Assert.IsType<AuctionDto>(createdResult.Value);
            Assert.Equal(createAuctionDto.Name, auctionDto.Name);
        }
    }
}
