using Backend_PcAuction.Controllers;
using Backend_PcAuction.Data.Dtos;
using Backend_PcAuction.Data.Entities;
using Backend_PcAuction.Data.Repositories;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    public class PartRequestsControllerTests
    {
        private readonly Mock<IPartRequestsRepository> _partRequestsRepositoryMock;
        private readonly Mock<IPartCategoriesRepository> _partCategoriesRepositoryMock;
        private readonly Mock<IPartsRepository> _partsRepositoryMock;
        private readonly Mock<IAuctionsRepository> _auctionsRepositoryMock;
        private readonly PartRequestsController _controller;

        public PartRequestsControllerTests()
        {
            _partRequestsRepositoryMock = new Mock<IPartRequestsRepository>();
            _partCategoriesRepositoryMock = new Mock<IPartCategoriesRepository>();
            _partsRepositoryMock = new Mock<IPartsRepository>();
            _auctionsRepositoryMock = new Mock<IAuctionsRepository>();

            _controller = new PartRequestsController(
                _partRequestsRepositoryMock.Object,
                _partCategoriesRepositoryMock.Object,
                _partsRepositoryMock.Object,
                _auctionsRepositoryMock.Object
            );
        }

        [Fact]
        public async Task Create_ReturnsCreated_WhenValidInput()
        {
            var createPartRequestDto = new CreatePartRequestDto("name", Guid.NewGuid(), "someCategoryId", Guid.NewGuid());

            var category = new PartCategory { Id = "categoryId" };
            var part = new Part { Id = createPartRequestDto.PartId, Category = category };
            var auction = new Auction();

            _partCategoriesRepositoryMock.Setup(repo => repo.GetAsync(createPartRequestDto.CategoryId)).ReturnsAsync(category);
            _partsRepositoryMock.Setup(repo => repo.GetAsync(category.Id, createPartRequestDto.PartId)).ReturnsAsync(part);
            _auctionsRepositoryMock.Setup(repo => repo.GetAsync(createPartRequestDto.AuctionId)).ReturnsAsync(auction);
            _partRequestsRepositoryMock.Setup(repo => repo.CreateAsync(It.IsAny<PartRequest>()));

            var result = await _controller.Create(createPartRequestDto);

            var createdResult = Assert.IsType<CreatedResult>(result.Result);
            var partRequestDto = Assert.IsType<PartRequestDto>(createdResult.Value);

            Assert.NotNull(partRequestDto);
            Assert.Equal(category.Id, partRequestDto.CategoryId);
        }

        [Fact]
        public async Task Create_ReturnsNotFound_WhenCategoryNotFound()
        {
            var createPartRequestDto = new CreatePartRequestDto("name", Guid.NewGuid(), "someCategoryId", Guid.NewGuid());

            _partCategoriesRepositoryMock.Setup(repo => repo.GetAsync(createPartRequestDto.CategoryId)).ReturnsAsync((PartCategory)null);

            var result = await _controller.Create(createPartRequestDto);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Create_ReturnsNotFound_WhenPartNotFound()
        {
            var createPartRequestDto = new CreatePartRequestDto("name", Guid.NewGuid(), "someCategoryId", Guid.NewGuid());

            var category = new PartCategory();
            _partCategoriesRepositoryMock.Setup(repo => repo.GetAsync(createPartRequestDto.CategoryId)).ReturnsAsync(category);
            _partsRepositoryMock.Setup(repo => repo.GetAsync(category.Id, createPartRequestDto.PartId)).ReturnsAsync((Part)null);

            var result = await _controller.Create(createPartRequestDto);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Create_ReturnsNotFound_WhenAuctionNotFound()
        {
            var createPartRequestDto = new CreatePartRequestDto("name", Guid.NewGuid(), "someCategoryId", Guid.NewGuid());

            var category = new PartCategory();
            var part = new Part();
            _partCategoriesRepositoryMock.Setup(repo => repo.GetAsync(createPartRequestDto.CategoryId)).ReturnsAsync(category);
            _partsRepositoryMock.Setup(repo => repo.GetAsync(category.Id, createPartRequestDto.PartId)).ReturnsAsync(part);
            _auctionsRepositoryMock.Setup(repo => repo.GetAsync(createPartRequestDto.AuctionId)).ReturnsAsync((Auction)null);

            var result = await _controller.Create(createPartRequestDto);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Get_ReturnsOkWithPartRequestDto_WhenPartRequestExists()
        {
            var partRequestId = Guid.NewGuid();
            var partRequest = new PartRequest
            {
                Id = partRequestId,
                Name = "Test Part Request",
                Auction = new Auction { Id = Guid.NewGuid() },
                Part = new Part { Id = Guid.NewGuid(), Category = new PartCategory { Id = "categoryId" } }
            };
            _partRequestsRepositoryMock.Setup(repo => repo.GetAsync(partRequestId)).ReturnsAsync(partRequest);

            var result = await _controller.Get(partRequestId);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var partRequestDto = Assert.IsType<PartRequestDto>(okResult.Value);
            Assert.Equal(partRequestId, partRequestDto.Id);
        }

        [Fact]
        public async Task Get_ReturnsNotFound_WhenPartRequestDoesNotExist()
        {
            var partRequestId = Guid.NewGuid();
            _partRequestsRepositoryMock.Setup(repo => repo.GetAsync(partRequestId)).ReturnsAsync((PartRequest)null);

            var result = await _controller.Get(partRequestId);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetMany_ReturnsOkWithPartRequestDtos_WhenPartRequestsExist()
        {
            var partId = Guid.NewGuid();
            var categoryId = "CPU";
            var auctionId = Guid.NewGuid();

            var expectedPartRequests = new List<PartRequest>
            {
                new PartRequest { Id = Guid.NewGuid(), Name = "PartRequest1", Part = new Part { Id = partId, Category = new PartCategory { Id = categoryId } }, Auction = new Auction { Id = auctionId } },
                new PartRequest { Id = Guid.NewGuid(), Name = "PartRequest2", Part = new Part { Id = partId, Category = new PartCategory { Id = categoryId } }, Auction = new Auction { Id = auctionId } }
            };

            _partRequestsRepositoryMock.Setup(repo => repo.GetManyAsync()).ReturnsAsync(expectedPartRequests);

            var result = await _controller.GetMany();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var partRequestDtos = Assert.IsAssignableFrom<IEnumerable<PartRequestDto>>(okResult.Value);
            Assert.Equal(expectedPartRequests.Count, partRequestDtos.Count());
        }


        [Fact]
        public async Task GetMany_ReturnsOkWithEmptyList_WhenNoPartRequestsExist()
        {
            _partRequestsRepositoryMock.Setup(repo => repo.GetManyAsync()).ReturnsAsync(new List<PartRequest>());

            var result = await _controller.GetMany();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var partRequestDtos = Assert.IsAssignableFrom<IEnumerable<PartRequestDto>>(okResult.Value);
            Assert.Empty(partRequestDtos);
        }

        [Fact]
        public async Task Delete_ReturnsNoContent_WhenPartRequestExists()
        {
            var partRequestId = Guid.NewGuid();
            var partRequest = new PartRequest { Id = partRequestId };
            _partRequestsRepositoryMock.Setup(repo => repo.GetAsync(partRequestId)).ReturnsAsync(partRequest);

            var result = await _controller.Delete(partRequestId);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenPartRequestDoesNotExist()
        {
            var partRequestId = Guid.NewGuid();
            _partRequestsRepositoryMock.Setup(repo => repo.GetAsync(partRequestId)).ReturnsAsync((PartRequest)null);

            var result = await _controller.Delete(partRequestId);

            Assert.IsType<NotFoundResult>(result);
        }

    }
}
