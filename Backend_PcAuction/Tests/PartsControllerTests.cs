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
    public class PartsControllerTests
    {
        private readonly Mock<IPartCategoriesRepository> _partCategoriesRepositoryMock;
        private readonly Mock<ISeriesRepository> _seriesRepositoryMock;
        private readonly Mock<IPartsRepository> _partsRepositoryMock;

        private readonly PartsController _controller;

        public PartsControllerTests()
        {
            _partCategoriesRepositoryMock = new Mock<IPartCategoriesRepository>();
            _seriesRepositoryMock = new Mock<ISeriesRepository>();
            _partsRepositoryMock = new Mock<IPartsRepository>();

            _controller = new PartsController(_partCategoriesRepositoryMock.Object, _seriesRepositoryMock.Object, _partsRepositoryMock.Object);
        }

        [Fact]
        public async Task Create_ReturnsNotFound_WhenCategoryNotFound()
        {
            var categoryId = "someCategoryId";
            _partCategoriesRepositoryMock.Setup(repo => repo.GetAsync(categoryId)).ReturnsAsync((PartCategory)null);

            var result = await _controller.Create(categoryId, new CreatePartDto("part1", "", "", "", "", "", "", "", "", "", "", Guid.NewGuid()));

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Create_ReturnsNotFound_WhenSeriesIdNotNullAndSeriesNotFound()
        {
            var categoryId = "someCategoryId";
            var createPartDto = new CreatePartDto("part1", "", "", "", "", "", "", "", "", "", "", Guid.NewGuid());
            var category = new PartCategory();
            _partCategoriesRepositoryMock.Setup(repo => repo.GetAsync(categoryId)).ReturnsAsync(category);
            _seriesRepositoryMock.Setup(repo => repo.GetAsync(categoryId, It.IsAny<Guid>())).ReturnsAsync((Series)null);

            var result = await _controller.Create(categoryId, createPartDto);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Create_ReturnsCreated_WhenCategoryAndSeriesFound()
        {
            var categoryId = "someCategoryId";
            var partName = "part1";
            var createPartDto = new CreatePartDto(partName, "", "", "", "", "", "", "", "", "", "", Guid.NewGuid());
            var category = new PartCategory(); 
            var series = new Series();
            _partCategoriesRepositoryMock.Setup(repo => repo.GetAsync(categoryId)).ReturnsAsync(category);
            _seriesRepositoryMock.Setup(repo => repo.GetAsync(categoryId, It.IsAny<Guid>())).ReturnsAsync(series);
            _partsRepositoryMock.Setup(repo => repo.CreateAsync(It.IsAny<Part>()));

            var result = await _controller.Create(categoryId, createPartDto);

            var createdResult = Assert.IsType<CreatedResult>(result.Result);
            var partDto = Assert.IsType<PartDto>(createdResult.Value);

            Assert.NotNull(partDto);
            Assert.NotNull(partDto.Id);
            Assert.Equal(partName, partDto.Name);
        }

        [Fact]
        public async Task Get_ReturnsNotFound_WhenPartNotFound()
        {
            var categoryId = "someCategoryId";
            var partId = Guid.NewGuid();
            var category = new PartCategory();
            _partCategoriesRepositoryMock.Setup(repo => repo.GetAsync(categoryId)).ReturnsAsync(category);
            _partsRepositoryMock.Setup(repo => repo.GetAsync(categoryId, partId)).ReturnsAsync((Part)null);

            var result = await _controller.Get(categoryId, partId);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Get_ReturnsOk_WhenCategoryAndPartFound()
        {
            var categoryId = "someCategoryId";
            var partId = Guid.NewGuid();
            var category = new PartCategory { Id = categoryId };
            var part = new Part { Id = partId, Name = "Test Part", Category = category };
            _partCategoriesRepositoryMock.Setup(repo => repo.GetAsync(categoryId)).ReturnsAsync(category);
            _partsRepositoryMock.Setup(repo => repo.GetAsync(categoryId, partId)).ReturnsAsync(part);

            var result = await _controller.Get(categoryId, partId);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var partDto = Assert.IsType<PartDto>(okResult.Value);
            Assert.Equal(part.Id, partDto.Id);
            Assert.Equal(part.Name, partDto.Name);
            Assert.Equal(categoryId, partDto.CategoryId);
        }

        [Fact]
        public async Task GetMany_ReturnsNotFound_WhenCategoryNotFound()
        {
            var categoryId = "someCategoryId";
            _partCategoriesRepositoryMock.Setup(repo => repo.GetAsync(categoryId)).ReturnsAsync((PartCategory)null);

            var result = await _controller.GetMany(categoryId);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetMany_ReturnsOkWithPartDtos_WhenCategoryFound()
        {
            var categoryId = "someCategoryId";
            var category = new PartCategory { Id = categoryId };
            var parts = new List<Part>
            {
                new Part { Id = Guid.NewGuid(), Name = "Part 1", Category = category },
                new Part { Id = Guid.NewGuid(), Name = "Part 2", Category = category }
            };
            _partCategoriesRepositoryMock.Setup(repo => repo.GetAsync(categoryId)).ReturnsAsync(category);
            _partsRepositoryMock.Setup(repo => repo.GetManyAsync(categoryId)).ReturnsAsync(parts);

            var result = await _controller.GetMany(categoryId);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var partDtos = Assert.IsAssignableFrom<IEnumerable<PartDto>>(okResult.Value);
            Assert.Equal(2, partDtos.Count());
        }

        [Fact]
        public async Task GetManyTemp_ReturnsNotFound_WhenCategoryNotFound()
        {
            var categoryId = "someCategoryId";
            _partCategoriesRepositoryMock.Setup(repo => repo.GetAsync(categoryId)).ReturnsAsync((PartCategory)null);

            var result = await _controller.GetManyTemp(categoryId);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetManyTemp_ReturnsOkWithPartDtos_WhenCategoryFound()
        {
            var categoryId = "someCategoryId";
            var category = new PartCategory { Id = categoryId };
            var parts = new List<Part>
            {
                new Part { Id = Guid.NewGuid(), Name = "Part 1", Category = category },
                new Part { Id = Guid.NewGuid(), Name = "Part 2", Category = category }
            };

            _partCategoriesRepositoryMock.Setup(repo => repo.GetAsync(categoryId)).ReturnsAsync(category);
            _partsRepositoryMock.Setup(repo => repo.GetManyTempAsync(categoryId)).ReturnsAsync(parts);

            var result = await _controller.GetManyTemp(categoryId);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var partDtos = Assert.IsAssignableFrom<IEnumerable<PartDto>>(okResult.Value);
            Assert.Equal(2, partDtos.Count());
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenPartNotFound()
        {
            var categoryId = "someCategoryId";
            var partId = Guid.NewGuid();
            var updatePartDto = new UpdatePartDto("", "", "", "", "", "", "", "", "", "", "", Guid.NewGuid(), "");
            var category = new PartCategory { Id = categoryId };
            _partCategoriesRepositoryMock.Setup(repo => repo.GetAsync(categoryId)).ReturnsAsync(category);
            _partsRepositoryMock.Setup(repo => repo.GetAsync(categoryId, partId)).ReturnsAsync((Part)null);

            var result = await _controller.Update(categoryId, partId, updatePartDto);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenSeriesNotFound()
        {
            var categoryId = "someCategoryId";
            var partId = Guid.NewGuid();
            var seriesId = Guid.NewGuid();
            var updatePartDto = new UpdatePartDto("", "", "", "", "", "", "", "", "", "", "", seriesId, "");
            var category = new PartCategory { Id = categoryId };
            var part = new Part { Id = partId };
            _partCategoriesRepositoryMock.Setup(repo => repo.GetAsync(categoryId)).ReturnsAsync(category);
            _partsRepositoryMock.Setup(repo => repo.GetAsync(categoryId, partId)).ReturnsAsync(part);
            _seriesRepositoryMock.Setup(repo => repo.GetAsync(categoryId, seriesId)).ReturnsAsync((Series)null);

            var result = await _controller.Update(categoryId, partId, updatePartDto);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Update_ReturnsOkWithPartDto_WhenCategoryPartAndSeriesFound()
        {
            var categoryId = "someCategoryId";
            var partId = Guid.NewGuid();
            var seriesId = Guid.NewGuid();
            var updatePartDto = new UpdatePartDto("", "", "", "", "", "", "", "", "", "", "", seriesId, "");
            var category = new PartCategory { Id = categoryId };
            var part = new Part { Id = partId, Category = category };
            var series = new Series { Id = seriesId };
            _partCategoriesRepositoryMock.Setup(repo => repo.GetAsync(categoryId)).ReturnsAsync(category);
            _partsRepositoryMock.Setup(repo => repo.GetAsync(categoryId, partId)).ReturnsAsync(part);
            _seriesRepositoryMock.Setup(repo => repo.GetAsync(categoryId, seriesId)).ReturnsAsync(series);
            _partsRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<Part>()));

            var result = await _controller.Update(categoryId, partId, updatePartDto);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var partDto = Assert.IsType<PartDto>(okResult.Value);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenCategoryNotFound()
        {
            var categoryId = "someCategoryId";
            _partCategoriesRepositoryMock.Setup(repo => repo.GetAsync(categoryId)).ReturnsAsync((PartCategory)null);

            var result = await _controller.Delete(categoryId, Guid.NewGuid());

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenPartNotFound()
        {
            var categoryId = "someCategoryId";
            var partId = Guid.NewGuid();
            var category = new PartCategory { Id = categoryId };
            _partCategoriesRepositoryMock.Setup(repo => repo.GetAsync(categoryId)).ReturnsAsync(category);
            _partsRepositoryMock.Setup(repo => repo.GetAsync(categoryId, partId)).ReturnsAsync((Part)null);

            var result = await _controller.Delete(categoryId, partId);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsNoContent_WhenCategoryAndPartFound()
        {
            var categoryId = "someCategoryId";
            var partId = Guid.NewGuid();
            var category = new PartCategory { Id = categoryId };
            var part = new Part { Id = partId };
            _partCategoriesRepositoryMock.Setup(repo => repo.GetAsync(categoryId)).ReturnsAsync(category);
            _partsRepositoryMock.Setup(repo => repo.GetAsync(categoryId, partId)).ReturnsAsync(part);
            _partsRepositoryMock.Setup(repo => repo.DeleteAsync(It.IsAny<Part>()));

            var result = await _controller.Delete(categoryId, partId);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenCategoryNotFound()
        {
            var categoryId = "someCategoryId";
            var partId = Guid.NewGuid();
            var updatePartDto = new UpdatePartDto("", "", "", "", "", "", "", "", "", "", "", Guid.NewGuid(), "");
            _partCategoriesRepositoryMock.Setup(repo => repo.GetAsync(categoryId)).ReturnsAsync((PartCategory)null);

            var result = await _controller.Update(categoryId, partId, updatePartDto);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Get_ReturnsNotFound_WhenCategoryNotFound()
        {
            var categoryId = "someCategoryId";
            var partId = Guid.NewGuid();
            _partCategoriesRepositoryMock.Setup(repo => repo.GetAsync(categoryId)).ReturnsAsync((PartCategory)null);

            var result = await _controller.Get(categoryId, partId);

            Assert.IsType<NotFoundResult>(result.Result);
        }
    }
}
