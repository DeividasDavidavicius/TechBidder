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
    public class PartCategoriesControllerTests
    {
        private readonly Mock<IPartCategoriesRepository> _partCategoriesRepositoryMock;
        private readonly PartCategoriesController _controller;

        public PartCategoriesControllerTests()
        {
            _partCategoriesRepositoryMock = new Mock<IPartCategoriesRepository>();
            _controller = new PartCategoriesController(_partCategoriesRepositoryMock.Object);
        }

        [Fact]
        public async Task Get_ReturnsOkWithPartCategoryDto_WhenCategoryExists()
        {
            var categoryId = "someCategoryId";
            var expectedCategory = new PartCategory
            {
                Id = "someCategoryId",
                SpecificationName1 = "Spec1",
                SpecificationName2 = "Spec2",
                SpecificationName3 = "Spec3",
                SpecificationName4 = "Spec4",
                SpecificationName5 = "Spec5",
                SpecificationName6 = "Spec6",
                SpecificationName7 = "Spec7",
                SpecificationName8 = "Spec8",
                SpecificationName9 = "Spec9",
                SpecificationName10 = "Spec10"
            };

            _partCategoriesRepositoryMock.Setup(repo => repo.GetAsync(categoryId)).ReturnsAsync(expectedCategory);

            var result = await _controller.Get(categoryId);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var categoryDto = Assert.IsType<PartCategoryDto>(okResult.Value);

            Assert.Equal(expectedCategory.Id, categoryDto.Id);
            Assert.Equal(expectedCategory.SpecificationName1, categoryDto.SpecificationName1);
            Assert.Equal(expectedCategory.SpecificationName2, categoryDto.SpecificationName2);
            Assert.Equal(expectedCategory.SpecificationName3, categoryDto.SpecificationName3);
            Assert.Equal(expectedCategory.SpecificationName4, categoryDto.SpecificationName4);
            Assert.Equal(expectedCategory.SpecificationName5, categoryDto.SpecificationName5);
            Assert.Equal(expectedCategory.SpecificationName6, categoryDto.SpecificationName6);
            Assert.Equal(expectedCategory.SpecificationName7, categoryDto.SpecificationName7);
            Assert.Equal(expectedCategory.SpecificationName8, categoryDto.SpecificationName8);
            Assert.Equal(expectedCategory.SpecificationName9, categoryDto.SpecificationName9);
            Assert.Equal(expectedCategory.SpecificationName10, categoryDto.SpecificationName10);
        }

        [Fact]
        public async Task Get_ReturnsNotFound_WhenCategoryDoesNotExist()
        {
            var categoryId = "nonExistentCategoryId";
            _partCategoriesRepositoryMock.Setup(repo => repo.GetAsync(categoryId)).ReturnsAsync((PartCategory)null);

            var result = await _controller.Get(categoryId);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetMany_ReturnsOkWithPartCategoryDtos_WhenCategoriesExist()
        {
            var expectedCategories = new List<PartCategory>
            {
                new PartCategory
                {
                    Id = "CPU",
                    SpecificationName1 = "Spec1",
                    SpecificationName2 = "Spec2",
                    SpecificationName3 = "Spec3",
                    SpecificationName4 = "Spec4",
                    SpecificationName5 = "Spec5",
                    SpecificationName6 = "Spec6",
                    SpecificationName7 = "Spec7",
                    SpecificationName8 = "Spec8",
                    SpecificationName9 = "Spec9",
                    SpecificationName10 = "Spec10"
                },
                new PartCategory
                {
                    Id = "GPU",
                    SpecificationName1 = "Spec1",
                    SpecificationName2 = "Spec2",
                    SpecificationName3 = "Spec3",
                    SpecificationName4 = "Spec4",
                    SpecificationName5 = "Spec5",
                    SpecificationName6 = "Spec6",
                    SpecificationName7 = "Spec7",
                    SpecificationName8 = "Spec8",
                    SpecificationName9 = "Spec9",
                    SpecificationName10 = "Spec10"
                }
            };

            _partCategoriesRepositoryMock.Setup(repo => repo.GetManyAsync()).ReturnsAsync(expectedCategories);

            var result = await _controller.GetMany();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var categoryDtos = Assert.IsAssignableFrom<IEnumerable<PartCategoryDto>>(okResult.Value);
            Assert.Equal(expectedCategories.Count, categoryDtos.Count());
        }

        [Fact]
        public async Task GetMany_ReturnsOkWithEmptyList_WhenNoCategoriesExist()
        {
            _partCategoriesRepositoryMock.Setup(repo => repo.GetManyAsync()).ReturnsAsync(new List<PartCategory>());

            var result = await _controller.GetMany();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var categoryDtos = Assert.IsAssignableFrom<IEnumerable<PartCategoryDto>>(okResult.Value);
            Assert.Empty(categoryDtos);
        }


    }
}
