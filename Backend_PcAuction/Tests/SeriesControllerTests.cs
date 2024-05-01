using Backend_PcAuction.Controllers;
using Backend_PcAuction.Data.Dtos;
using Backend_PcAuction.Data.Entities;
using Backend_PcAuction.Data.Repositories;
using Backend_PcAuction.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    public class SeriesControllerTests
    {
        private readonly Mock<IPartCategoriesRepository> _partCategoriesRepositoryMock;
        private readonly Mock<ISeriesRepository> _seriesRepositoryMock;

        private readonly SeriesController _controller;

        public SeriesControllerTests()
        {
            _partCategoriesRepositoryMock = new Mock<IPartCategoriesRepository>();
            _seriesRepositoryMock = new Mock<ISeriesRepository>();
            _controller = new SeriesController(_partCategoriesRepositoryMock.Object, _seriesRepositoryMock.Object);
        }

        [Fact]
        public async Task Create_ValidCategory_ReturnsCreated()
        {
            var categoryId = "categoryId";
            var category = new PartCategory { Id = categoryId };
            _partCategoriesRepositoryMock.Setup(repo => repo.GetAsync(categoryId)).ReturnsAsync(category);

            var seriesId = Guid.NewGuid();
            var seriesName = "seriesName";
            var createSeriesDto = new CreateSeriesDto(seriesName);
            var series = new Series { Id = seriesId, Name = seriesName, PartCategory = category };
            _seriesRepositoryMock.Setup(repo => repo.CreateAsync(It.IsAny<Series>()));

            var result = await _controller.Create(categoryId, createSeriesDto);

            var createdResult = Assert.IsType<CreatedResult>(result.Result);
            var seriesDto = Assert.IsType<SeriesDto>(createdResult.Value);

            Assert.Equal(seriesName, seriesDto.Name);
        }

        [Fact]
        public async Task Create_InvalidCategory_ReturnsCreated()
        {
            var categoryId = "categoryId";
            var category = new PartCategory { Id = categoryId };

            var seriesName = "seriesName";
            var createSeriesDto = new CreateSeriesDto(seriesName);
            _seriesRepositoryMock.Setup(repo => repo.CreateAsync(It.IsAny<Series>()));

            var result = await _controller.Create(categoryId, createSeriesDto);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Get_ExistingCategoryAndSeries_ReturnsOk()
        {
            var categoryId = "categoryId";
            var seriesId = Guid.NewGuid();
            var seriesName = "seriesName";
            var category = new PartCategory { Id = categoryId };
            var series = new Series { Id = seriesId, Name = seriesName, PartCategory = category };

            _partCategoriesRepositoryMock.Setup(repo => repo.GetAsync(categoryId)).ReturnsAsync(category);
            _seriesRepositoryMock.Setup(repo => repo.GetAsync(categoryId, seriesId)).ReturnsAsync(series);

            var result = await _controller.Get(categoryId, seriesId);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var seriesDto = Assert.IsType<SeriesDto>(okResult.Value);
            Assert.Equal(seriesId, seriesDto.Id);
            Assert.Equal(seriesName, seriesDto.Name);
        }

        [Fact]
        public async Task Get_NonExistingCategory_ReturnsNotFound()
        {
            var categoryId = "nonExistingCategoryId";
            var seriesId = Guid.NewGuid();

            _partCategoriesRepositoryMock.Setup(repo => repo.GetAsync(categoryId)).ReturnsAsync((PartCategory)null);

            var result = await _controller.Get(categoryId, seriesId);

            var notFoundResult = Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Get_ExistingCategoryAndNonExistingSeries_ReturnsNotFound()
        {
            var categoryId = "categoryId";
            var seriesId = Guid.NewGuid();
            var category = new PartCategory { Id = categoryId };

            _partCategoriesRepositoryMock.Setup(repo => repo.GetAsync(categoryId)).ReturnsAsync(category);
            _seriesRepositoryMock.Setup(repo => repo.GetAsync(categoryId, seriesId)).ReturnsAsync((Series)null);

            var result = await _controller.Get(categoryId, seriesId);

            var notFoundResult = Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetMany_ExistingCategory_ReturnsOkWithSeriesDtoList()
        {
            var categoryId = "categoryId";
            var categoryName = "categoryName";
            var category = new PartCategory { Id = categoryId };

            var seriesList = new List<Series>
            {
                new Series { Id = Guid.NewGuid(), Name = "seriesName1", PartCategory = category },
                new Series { Id = Guid.NewGuid(), Name = "seriesName2", PartCategory = category }
            };

            _partCategoriesRepositoryMock.Setup(repo => repo.GetAsync(categoryId)).ReturnsAsync(category);
            _seriesRepositoryMock.Setup(repo => repo.GetManyAsync(categoryId)).ReturnsAsync(seriesList);

            var result = await _controller.GetMany(categoryId);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var seriesDtoList = Assert.IsAssignableFrom<IEnumerable<SeriesDto>>(okResult.Value);
            Assert.Equal(seriesList.Count, seriesDtoList.Count());
            foreach (var seriesDto in seriesDtoList)
            {
                var series = seriesList.FirstOrDefault(s => s.Id == seriesDto.Id);
                Assert.NotNull(series);
                Assert.Equal(series.Name, seriesDto.Name);
            }
        }

        [Fact]
        public async Task GetMany_NonExistingCategory_ReturnsNotFound()
        {
            var categoryId = "nonExistingCategoryId";
            _partCategoriesRepositoryMock.Setup(repo => repo.GetAsync(categoryId)).ReturnsAsync((PartCategory)null);

            var result = await _controller.GetMany(categoryId);

            var notFoundResult = Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Update_ExistingCategoryAndSeries_ReturnsOkWithUpdatedSeriesDto()
        {
            var categoryId = "categoryId";
            var categoryName = "categoryName";
            var seriesId = Guid.NewGuid();
            var seriesName = "seriesName";

            var category = new PartCategory { Id = categoryId };
            var series = new Series { Id = seriesId, Name = seriesName, PartCategory = category };

            var updateSeriesDto = new UpdateSeriesDto("updatedSeriesName");

            _partCategoriesRepositoryMock.Setup(repo => repo.GetAsync(categoryId)).ReturnsAsync(category);
            _seriesRepositoryMock.Setup(repo => repo.GetAsync(categoryId, seriesId)).ReturnsAsync(series);

            var result = await _controller.Update(categoryId, seriesId, updateSeriesDto);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var seriesDto = Assert.IsType<SeriesDto>(okResult.Value);
            Assert.Equal(seriesId, seriesDto.Id);
            Assert.Equal(updateSeriesDto.Name, seriesDto.Name);

            Assert.Equal(updateSeriesDto.Name, series.Name);
            _seriesRepositoryMock.Verify(repo => repo.UpdateAsync(series), Times.Once);
        }

        [Fact]
        public async Task Update_NonExistingCategory_ReturnsNotFound()
        {
            var categoryId = "nonExistingCategoryId";
            var seriesId = Guid.NewGuid();
            var updateSeriesDto = new UpdateSeriesDto("updatedSeriesName");

            _partCategoriesRepositoryMock.Setup(repo => repo.GetAsync(categoryId)).ReturnsAsync((PartCategory)null);

            var result = await _controller.Update(categoryId, seriesId, updateSeriesDto);

            var notFoundResult = Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Update_ExistingCategoryAndNonExistingSeries_ReturnsNotFound()
        {
            var categoryId = "categoryId";
            var seriesId = Guid.NewGuid();
            var category = new PartCategory { Id = categoryId };

            var updateSeriesDto = new UpdateSeriesDto("updatedSeriesName");

            _partCategoriesRepositoryMock.Setup(repo => repo.GetAsync(categoryId)).ReturnsAsync(category);
            _seriesRepositoryMock.Setup(repo => repo.GetAsync(categoryId, seriesId)).ReturnsAsync((Series)null);

            var result = await _controller.Update(categoryId, seriesId, updateSeriesDto);

            var notFoundResult = Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Delete_ExistingCategoryAndSeries_ReturnsNoContent()
        {
            var categoryId = "categoryId";
            var categoryName = "categoryName";
            var seriesId = Guid.NewGuid();
            var seriesName = "seriesName";

            var category = new PartCategory { Id = categoryId };
            var series = new Series { Id = seriesId, Name = seriesName, PartCategory = category };

            _partCategoriesRepositoryMock.Setup(repo => repo.GetAsync(categoryId)).ReturnsAsync(category);
            _seriesRepositoryMock.Setup(repo => repo.GetAsync(categoryId, seriesId)).ReturnsAsync(series);

            var result = await _controller.Delete(categoryId, seriesId);

            var noContentResult = Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, noContentResult.StatusCode);

            _seriesRepositoryMock.Verify(repo => repo.DeleteAsync(series), Times.Once);
        }

        [Fact]
        public async Task Delete_NonExistingCategory_ReturnsNotFound()
        {
            var categoryId = "nonExistingCategoryId";
            var seriesId = Guid.NewGuid();

            _partCategoriesRepositoryMock.Setup(repo => repo.GetAsync(categoryId)).ReturnsAsync((PartCategory)null);

            var result = await _controller.Delete(categoryId, seriesId);

            var notFoundResult = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_ExistingCategoryAndNonExistingSeries_ReturnsNotFound()
        {
            var categoryId = "categoryId";
            var seriesId = Guid.NewGuid();
            var category = new PartCategory { Id = categoryId };

            _partCategoriesRepositoryMock.Setup(repo => repo.GetAsync(categoryId)).ReturnsAsync(category);
            _seriesRepositoryMock.Setup(repo => repo.GetAsync(categoryId, seriesId)).ReturnsAsync((Series)null);

            var result = await _controller.Delete(categoryId, seriesId);

            var notFoundResult = Assert.IsType<NotFoundResult>(result);
        }
    }
}
